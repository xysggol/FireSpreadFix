# Fire Spread Fix 火焰蔓延修复

A bug-fix mod for [Survivalcraft API](https://gitee.com/SC-SPM/SurvivalcraftApi) 1.9.x.

一个针对 [Survivalcraft API](https://gitee.com/SC-SPM/SurvivalcraftApi) 1.9.x 的 Bug 修复模组。

---

## The problem this mod solves / 本模组解决的问题

In vanilla (including the plugin/API edition), two flammable blocks separated by a non-flammable block can still ignite each other: when one is on fire, the other has a chance to catch fire **through the blocker** (see the screenshot: two wooden planks on both sides of a brick pillar).

在原版（包括插件版）中，两个可燃方块之间即使隔着不可燃方块，也会互相点燃：一侧燃烧时，另一侧有概率**穿过阻挡**被点燃（见截图：砖柱两侧的木板）。

Root cause: `SubsystemFireBlockBehavior.Update` takes far expansion offsets (up to ~2.5 blocks, e.g. `(±2, 0, 0)`) from `m_expansionProbabilities` and calls `SetCellOnFire` on the target block **without any line-of-sight check**, so fire can reach a flammable block directly behind a wall.

根因：`SubsystemFireBlockBehavior.Update` 通过 `m_expansionProbabilities` 取到最远约 2.5 格的蔓延偏移（例如 `(±2,0,0)`），随后直接对目标方块调用 `SetCellOnFire`，**完全没有阻挡/视线检测**，因此火焰可以点燃墙正后方的可燃方块。

## How it works / 实现原理

Via HarmonyX, this mod replaces `SubsystemFireBlockBehavior.Update` (Prefix that skips the original). It keeps the vanilla fire behaviour (timers, burn-away, particles, sound) but adds one check to each probabilistic expansion attempt:

本模组通过 HarmonyX 接管 `SubsystemFireBlockBehavior.Update`（Prefix 返回 `false` 跳过原版）。它保留原版火焰行为（计时、燃尽、粒子、音效），只对每次概率蔓延加一道判定：

- The fire must have an **unobstructed path** to the target flammable block: a chain of passable cells (air / fire / block 61) whose step count does not exceed the rounded Euclidean distance between the two cells.
- 火焰到目标可燃方块之间必须存在一条**不被实心方块阻挡的连通路径**：路径由可通行格（空气 / 火焰 / 方块 61）组成，步数不超过两者欧氏距离向上取整。

Consequences:

- Fire no longer jumps straight through a 1-block-thick wall (the reported bug).
- Sealed diagonal corners are blocked too (both orthogonal neighbours solid).
- Legitimate spread is preserved: adjacent blocks, open-air gaps, diagonals with an opening, and going over the top of a wall.

效果：

- 火焰不再隔着一格厚的墙直接跳燃（即所报 Bug）。
- 斜角被完全封死时同样不会穿墙。
- 合法蔓延保留：紧邻方块、隔空、有开口的斜向、以及从墙顶绕过。

## Compatibility / 兼容性

- Built and tested against [Survivalcraft API 1.9.3.1](https://gitee.com/SC-SPM/SurvivalcraftApi/releases/tag/API_1.9.3.1).
- Windows / Linux / Android share a single `.scmod`. No saved data, `NonPersistentMod` is `true` (safe to remove at any time).

- 基于 [Survivalcraft API 1.9.3.1](https://gitee.com/SC-SPM/SurvivalcraftApi/releases/tag/API_1.9.3.1) 构建与测试。
- Windows / Linux / Android 共用一个 `.scmod`。不写入存档（`NonPersistentMod=true`），可随时移除。

## Installation / 安装

1. Download `FireSpreadFix_API193.scmod` from Releases.
2. **PC (Windows / Linux)**: put the `.scmod` into the game's `Mods/` folder.
3. **Android**: push it to `/storage/emulated/0/Survivalcraft2.4_API1.9/Mods`, or simply open the file on the device to install it.

1. 从 Releases 下载 `FireSpreadFix_API193.scmod`。
2. **PC（Windows / Linux）**：把 `.scmod` 放进游戏的 `Mods/` 目录。
3. **Android**：用 `adb push` 放到 `/storage/emulated/0/Survivalcraft2.4_API1.9/Mods`，或在设备上直接打开该文件安装。

## Build from source / 从源码构建

Requires the .NET 10 SDK. Needs `nuget.config` in the project folder to restore `SurvivalcraftAPI.Survivalcraft`.

需要 .NET 10 SDK。`nuget.config` 须与项目同目录，用于还原 `SurvivalcraftAPI.Survivalcraft`。

```bash
dotnet build FireSpreadFix_API193.csproj -c Release
```

On success, `FireSpreadFix_API193.scmod` is generated in `bin/Release/`.

构建成功后，`FireSpreadFix_API193.scmod` 生成于 `bin/Release/`。

## License / 许可

LGPL-3.0 (`LICENSE`), with the GPL-3.0 full text in `LICENSE.GPL-3.0`.

LGPL-3.0（见 `LICENSE`），GPL-3.0 全文见 `LICENSE.GPL-3.0`。
