# FireSpreadFix_API193 Agent 指南

本仓库是《生存战争》插件版（SCAPI）模组 **火焰蔓延修复 / Fire Spread Fix** 的源码与发布仓库。

模组作用：修复原版火焰可穿过不可燃方块点燃另一侧可燃方块的 Bug。

## 基本信息

| 项 | 值 |
| --- | --- |
| 模组名 | 火焰蔓延修复 Fire Spread Fix |
| PackageName | `FireSpreadFix` |
| 版本 | 1.0.0 |
| 目标 API | Survivalcraft API `1.9.3.1`（兼容 1.9.x） |
| 目标框架 | `net10.0` |
| 平台 | Windows / Linux / Android（单个 `.scmod` 通用） |
| 许可 | LGPL-3.0（`LICENSE`，另附 GPL-3.0 全文 `LICENSE.GPL-3.0`） |

## GitHub 信息

| 项 | 值 |
| --- | --- |
| GitHub 账号 | `xysggol` |
| 仓库 | https://github.com/xysggol/FireSpreadFix |
| 默认分支 | `main`（本地分支同名，推送用 `git push origin main`） |
| 提交身份 | `xysgg` `<225452402+xysggol@users.noreply.github.com>` |
| Releases | https://github.com/xysggol/FireSpreadFix/releases |

### 邮箱与隐私（重要）

- 账号开启了「阻止命令行推送暴露我的邮箱」（GH007），**禁止**用私有邮箱 `xysgg_new@outlook.com` 提交/推送。
- 提交必须使用 GitHub noreply 邮箱：`225452402+xysggol@users.noreply.github.com`（即 `用户ID+用户名@users.noreply.github.com`）。
- 本仓库已设置 `user.name=xysgg`、`user.email=225452402+xysggol@users.noreply.github.com`。

### SSH 认证（本机已配置）

- 密钥：`~/.ssh/id_ed25519`（公钥已注册到 GitHub 账号）。
- 本机 22 端口被拒，`~/.ssh/config` 已把 GitHub 指向 443：

  ```
  Host github.com
      HostName ssh.github.com
      Port 443
      User git
      IdentityFile ~/.ssh/id_ed25519
      IdentitiesOnly yes
  ```

- 验证：`ssh -T git@github.com` 应返回 `Hi xysggol! ...`。
- 全局地址改写（让 `https://github.com/...` 自动走 SSH）：

  ```
  git config --global url."git@github.com:".insteadOf "https://github.com/"
  ```

### GPG 签名（本机已配置）

- 签名密钥：`ed25519 D63479F50BA4B12A`
- 指纹：`8215CF979D9A483449BA5982D63479F50BA4B12A`
- UID：`xysgg <225452402+xysggol@users.noreply.github.com>`，**无口令**
- 全局已开启：`commit.gpgsign=true`、`tag.gpgsign=true`、`user.signingkey=<指纹>`、`gpg.program=gpg`
- 验证签名：`git log --show-signature`；打签名标签：`git tag -s v1.0.1 -m "..."`。

#### 把 GPG 公钥加入 GitHub（否则提交/标签显示 Unverified）

未把公钥加入账号时，GitHub 上会显示 `verified: False, reason: unknown_key`。本仓库当前提交即为此状态，需按下列步骤手动添加公钥（token 无 `admin:gpg_key` 权限，无法自动添加）。

1. 打开 https://github.com/settings/gpg/new （或 `Settings → SSH and GPG keys → New GPG key`）。
2. 粘贴下面整段公钥，点 **Add GPG key**：

   ```
   -----BEGIN PGP PUBLIC KEY BLOCK-----

   mDMEaq+34BYJKwYBBAHaRw8BAQdARZq1hvLGmUxKrvmQ4Yubn24JfuTAGaiMu0FC
   EGjhoCC0Mnh5c2dnIDwyMjU0NTI0MDIreHlzZ2dvbEB1c2Vycy5ub3JlcGx5Lmdp
   dGh1Yi5jb20+iJMEExYKADsWIQSCFc+XnZpINEm6WYLWNHn1C6SxKgUCaq+34AIb
   AwULCQgHAgIiAgYVCgkICwIEFgIDAQIeBwIXgAAKCRDWNHn1C6SxKoexAP9nBJBh
   i2f4QArlwgQwXQu2N6lZwTFB4017l+/0ag33yQEA+YjVuTURnfNWbMGLDNSE4gDT
   zgYN1stUau6tBWoYMAg=
   =refa
   -----END PGP PUBLIC KEY BLOCK-----
   ```

3. 添加后，该密钥签名的提交/标签（包括已推送的历史提交）都会变为 **Verified**。

   导出公钥命令（如需重新导出）：`gpg --armor --export 8215CF979D9A483449BA5982D63479F50BA4B12A`

### GitHub CLI

- 本机 `gh` 位于 `~/.local/bin/gh`，已登录账号 `xysggol`（token 存于系统 keyring，scope 含 `repo`）。
- 常用：
  - 刷新/重传发行附件：`gh release upload <tag> <files...> --clobber`
  - 更新发行说明：`gh release edit <tag> --notes-file <notes.md>`
  - 改标题：`gh release edit <tag> --title "..."`
- **不要把 token、私钥、口令写入任何仓库文件。**

## 文档约定（重要，务必遵守）

- **中英双语文档必须拆成两个文件，禁止混写在一个文件里**：
  - `README.md`：纯英文；
  - `README.zh-CN.md`：纯中文。
- `README.md` 必须**零中文字符、零中文标点**（包括 `。，、：；！？（）「」` 等全角符号）。语言切换链接写 `[Chinese](README.zh-CN.md)`，**不要**写 `[中文说明 / Chinese]` 这类混入中文的写法。英文正文里的非 ASCII 符号（如 `±`）也一律替换为 ASCII（如 `+/-`）。
- `README.zh-CN.md` 用中文撰写，语言切换链接写 `[English](README.md)`；除代码、术语、链接外不要整段照搬英文。
- 两个文件内容需保持同步；改动其一时同时更新另一个。
- 提交前自检（应无输出）：`git grep -nP "[\x{4e00}-\x{9fff}\x{3000}-\x{303f}\x{ff00}-\x{ffef}]" -- README.md`。
- 每个 README 首次出现「Survivalcraft API」（中文版为「生存战争插件版」）时必须超链接到仓库地址 `https://gitee.com/SC-SPM/SurvivalcraftApi`，避免歧义；提到版本时可另链到对应 release tag。
- 新增同类模组仓库时沿用 `.github/workflows/release.yml` 的 tag 发布方式。

## 目录结构

```
FireSpreadFix_API193.csproj              构建与打包（PostBuild 生成 .scmod）
FireSpreadFixModLoader.cs               ModLoader，__ModInitialize 里 Harmony.PatchAll
SubsystemFireBlockBehavior_Update_Patch.cs  Harmony Prefix：核心修复逻辑
modinfo.json                             模组元数据（Name/Version/ApiVersion/PackageName 等）
nuget.config                             NuGet 源（nuget.org + SurvivalcraftAPI 私有源）
AGENTS.md                                本文件
README.md / README.zh-CN.md              英文 / 中文说明
LICENSE / LICENSE.GPL-3.0                LGPL-3.0 / GPL-3.0 全文
.github/workflows/release.yml            打 tag 自动构建与发布
```

## 核心实现（改代码前先读这里）

原版 `SubsystemFireBlockBehavior.Update` 从 `m_expansionProbabilities` 取蔓延偏移（最远约 2.5 格，如 `(±2,0,0)`、`(0,2,0)`），然后直接对目标调用 `SetCellOnFire`；`SetCellOnFire` 只判断目标是否可燃，不判断中间是否被阻挡，因此火焰会「隔墙传火」。

`SubsystemFireBlockBehavior_Update_Patch.cs` 对 `SubsystemFireBlockBehavior.Update` 加 Harmony **Prefix**（返回 `false` 跳过原版），完整复刻原逻辑，仅在每次概率蔓延插入 `IsFireExpansionAllowed` 判定：火焰格与目标可燃格之间必须存在一条由可通行格（contents 为 `0`/`104`/`61`）组成、步数不超过两者欧氏距离向上取整的连通路径。

要点：
- 兼容的可通行方块集合与原版 `SetCellOnFire` 一致（`0`、`104`、`61`）。
- 只拦「无路可达」的蔓延，紧邻、隔空、有开口的斜向、绕过墙顶等合法蔓延保留。
- 判定用 BFS/DFS，深度上限 `ceil(欧氏距离)`（最大约 3），仅在随机概率命中后执行，开销可忽略。
- 为便于无窗口单测，核心判定提供接受 `Func<int,int,int,int>` 取值的 `internal` 重载；游戏内调用 `SubsystemTerrain` 重载。
- 异常要捕获并 `Log.Error`，不要让游戏崩溃。

## 构建

需要 **.NET 10 SDK**。

```bash
dotnet build FireSpreadFix_API193.csproj -c Release
```

- 产物：`bin/Release/FireSpreadFix_API193.scmod`（内部仅含 `FireSpreadFix_API193.dll`、`modinfo.json` 与 pdb，**不含 README**）。
- 还原依赖需要本目录的 `nuget.config`；私有源 `https://nuget.fury.io/survivalcraftapi` 可匿名只读拉取，无需凭据。
- `bin/`、`obj/` 已被 `.gitignore` 忽略，**不要提交构建产物**。

## 发布流程

在 GitHub 上由 Actions 构建并发布，本机无需上传：

```bash
git tag -s v1.0.1 -m "火焰蔓延修复 v1.0.1"
git push origin v1.0.1
```

推送 `v*` tag 会触发 `.github/workflows/release.yml`：在 `ubuntu-latest` 上 `dotnet build`，生成 `.scmod` 与 `.sha256`，再 `gh release create` 建 release 并挂附件（若 release 已存在则 `gh release upload --clobber`）。也可在仓库 Actions 页手动 `Run workflow` 仅测试构建。

注意：改 README 不会改变 `.scmod` 内容；如需刷新已有 release 的附件/说明，用上文 `gh release upload` / `gh release edit`。

## 调试与验证

- **无窗口运行游戏**：Linux 版没有可用的无头显示（`Display.InitializeHeadless` 仅 `#if WINDOWS`）。
  - 在本机 `DISPLAY=:0` 的真实 Wayland 会话下直接跑会弹窗，**不要**这样做。
  - Xvfb 也不行：游戏使用 OpenGL ES + EGL，GLFW 在 Xvfb 上创建上下文时段错误（exit 139）。
  - 需要真正无窗口渲染时，应使用无头 Wayland 合成器（weston headless backend）而非 Xvfb。
- 不依赖窗口的验证方式：
  - `dotnet build` 保证编译通过；
  - 用临时控制台程序 `Assembly.LoadFrom` 加载 `FireSpreadFix_API193.dll`，`Harmony.PatchAll(assembly)` 后检查 `Harmony.GetPatchInfo(typeof(SubsystemFireBlockBehavior).Update)`，并通过反射调用 `IsFireExpansionAllowed(Func<int,int,int,int>, ...)` 跑各类遮挡用例。
- 不要长时间前台阻塞式运行游戏后才发现没输出；调试游戏时应把日志重定向到文件并分次 `cat` 查看，或边跑边输出。

## 代码风格

遵循 `SurvivalcraftApi/.editorconfig`：4 空格缩进、K&R 大括号、最大行宽 150、`Nullable disable`、`LangVersion preview`、避免 `var`、公有成员 PascalCase。异常要捕获并 `Log.Error`，不要让游戏崩溃。

## 需要向用户确认的情形

- 任何会改写远端历史的操作（force push、删 tag/release）。
- 版本号、modinfo 字段、许可协议的变更。
- 仓库名 / 可见性（public/private）的决定。
- 是否需要 Android 实机验证（本机无设备，只能编译）。
