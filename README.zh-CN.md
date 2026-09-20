# 火焰蔓延修复

一个用于[生存战争插件版](https://gitee.com/SC-SPM/SurvivalcraftApi)（SCAPI 1.9.x）的 Bug 修复模组。

[English](README.md)

---

## 这个模组解决什么问题

在原版（包括插件版）中，火焰蔓延时会无视不可燃方块：当某个可燃方块正在燃烧时，距离它约 2.5 格以内的另一个可燃方块，**即使两者之间正隔着一块实心的不可燃方块**，也有概率被点燃：

> 砖柱两侧的两块木板会互相点燃。

该现象对任何可燃方块都成立，不只限于木板。

## 根本原因

`SubsystemFireBlockBehavior.Update` 从 `m_expansionProbabilities` 中取出蔓延目标偏移，该表最远约 2.5 格（例如 `(±2, 0, 0)`、`(0, 2, 0)`），随后直接对目标调用 `SetCellOnFire`。

而 `SetCellOnFire` 只检查目标方块是否可燃，从不检查火焰与目标之间的路径是否被阻挡，于是火焰可以“穿墙”蔓延。

## 修复原理

本模组通过 HarmonyX 接管 `SubsystemFireBlockBehavior.Update`（Prefix 返回 `false` 跳过原版）。原版行为全部保留（计时、燃尽、粒子、音效），只对每次概率蔓延追加一道判定：

- 火焰格到目标可燃方块之间必须存在一条**不被实心方块阻挡的连通路径**：路径由可通行格（空气 / 火焰 / 方块 61）组成，步数不超过两者欧氏距离向上取整。

修复后的行为：

| 情形 | 原版 | 本模组 |
| --- | --- | --- |
| 可燃方块正后方隔着一格厚的墙 | 有概率点燃 | 不点燃 |
| 斜角被完全封死（两个正交邻居都是实心） | 有概率点燃 | 不点燃 |
| 紧邻的可燃方块 | 点燃 | 点燃 |
| 隔空、有开口的斜向、从墙顶绕过的路径 | 点燃 | 点燃 |

本模组只增加了阻挡判定，点燃半径、概率与计时均未改动，正常火焰蔓延的手感与原版一致。

## 兼容性与限制

- 基于[生存战争插件版 1.9.3.1](https://gitee.com/SC-SPM/SurvivalcraftApi/releases/tag/API_1.9.3.1) 构建与测试。
- 覆盖所有经由火焰子系统点火的方式：火焰方块、火柴、爆炸、燃烧投掷物、岩浆等。
- 不新增方块、物品或实体，不写入存档（`NonPersistentMod=true`），可随时移除且不会有缺失模组警告。
- Windows / Linux / Android 共用一个 `.scmod`。

## 安装

1. 从 Releases 下载 `FireSpreadFix_API193.scmod`。
2. **PC（Windows / Linux）**：把 `.scmod` 放入游戏目录下的 `Mods/` 文件夹。
3. **Android**：把 `.scmod` 推送到 `/storage/emulated/0/Survivalcraft2.4_API1.9/Mods`，或在设备上直接打开该文件安装。

## 从源码构建

需要 .NET 10 SDK。项目需要自带的 `nuget.config` 才能还原 `SurvivalcraftAPI.Survivalcraft`。

```bash
dotnet build FireSpreadFix_API193.csproj -c Release
```

构建成功后会在 `bin/Release/` 下生成 `FireSpreadFix_API193.scmod`。

## 发布新版本（维护者）

推送一个匹配 `v*` 的 tag 会触发 `.github/workflows/release.yml`：由 GitHub 的 runner 构建 `.scmod`，并自动把模组文件与 `.sha256` 校验文件挂到对应的 GitHub Release 上，本机无需上传任何东西：

```bash
git tag v1.0.0
git push origin v1.0.0
```

## 许可证

本项目以 **GNU Lesser General Public License v3.0（LGPL-3.0）** 发布，详见 [LICENSE](LICENSE)。LGPL-3.0 内含对 GNU GPL v3.0 的引用，其完整文本见 [LICENSE.GPL-3.0](LICENSE.GPL-3.0)。
