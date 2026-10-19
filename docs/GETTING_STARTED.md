# 运行与构建指南

[返回首页](../README.md)

## 1. 准备环境

| 项目 | 版本 / 要求 | 依据 |
| --- | --- | --- |
| Unity Editor | **2022.3.62f3c1** | [ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt) |
| 渲染管线 | URP **14.0.12**，2D Renderer | [manifest.json](../Packages/manifest.json) |
| 输入 | 旧版 Input Manager | [InputManager.asset](../ProjectSettings/InputManager.asset) |
| 摄像机 | Cinemachine，经包依赖还原 | [manifest.json](../Packages/manifest.json) |
| 构建脚本目标 | Windows x86_64 | [BuildScript.cs](../Assets/Editor/BuildScript.cs) |

优先安装版本号完全一致的编辑器，包含 `c1` 后缀。需要升级或替换版本时，在独立分支验证后再提交 `ProjectSettings` 和包锁文件的变化。Windows 构建还需要相应平台支持模块。

## 2. 获取并打开项目

```bash
git clone https://github.com/Wangwu-1/-2d-dungeon-rpg.git dungeon-rpg
cd dungeon-rpg
```

1. 在 Unity Hub 的项目页选择添加已有项目，选中包含 `Assets`、`Packages`、`ProjectSettings` 的 `dungeon-rpg` 根目录。
2. 用指定版本的 Editor 打开项目。首次导入需要联网还原依赖，等待 Package Manager 和脚本编译结束。
3. 在 Project 窗口打开 **`Assets/Real.unity`**，这是 [EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset) 登记的构建入口。
4. 点击 Play，再点击 Game 窗口获得键鼠焦点。基本操作见[首页操作表](../README.md#操作)。

直接从入口场景开始；资源目录中的 Demo、场景模板和历史原型不作为当前运行入口。

## 3. 首次运行核对

- Console 中没有阻止运行的编译错误，场景中没有 Missing Script。
- 玩家可以左右移动、跳跃、冲刺，并在合适墙体进行墙滑与墙跳。
- 地面点击鼠标左键能进入攻击；核对连击、敌人受击和玩家生命变化。如需显示血条，先核对 Slider 与 `HealthBarController` 的场景接入。
- 接触尖刺时核对扣血、无敌时间及未死亡时的安全点回传。
- 进入已配置的房间出口时核对淡出、传送、淡入和输入恢复。
- 生命归零后，当前代码会停止玩家操作；再次测试可停止 Play 后重新进入。

这些是人工验收步骤，不代表本次文档整理已在 Unity 中执行过。

## 4. 构建 Windows 版本

在 Unity 中打开 **File → Build Settings**，确认启用 `Assets/Real.unity`，选择 Windows 的 x86_64 目标，构建到 `Build/` 或 `Builds/`。

项目也提供静态入口 `BuildScript.BuildGame`。Windows PowerShell 中可按实际安装位置填写 `$unityEditor`：

```powershell
$unityEditor = 'C:\Path\To\2022.3.62f3c1\Editor\Unity.exe'
$projectPath = (Get-Location).Path
& $unityEditor -batchmode -quit -projectPath $projectPath `
    -executeMethod BuildScript.BuildGame -logFile build.log
```

从项目根目录执行，先关闭打开同一项目的 Editor。现有脚本固定输出 **`Build/Game.exe`**，目标为 `StandaloneWindows64`；核对日志中的构建结果和实际输出。脚本仅记录失败结果，尚未显式设置失败退出码，不应只凭进程返回码判断构建成功。

构建产物不提交到源码仓库；需要分发试玩版时，单独准备发布文件并核对素材声明。

## 5. 常见问题

| 现象 | 先检查 |
| --- | --- |
| 提示编辑器版本不一致 | 对照 `ProjectVersion.txt` 的完整版本，包括 `c1` |
| Package Manager 导入失败 | 网络、包还原错误及 `Packages/manifest.json`；保留 `packages-lock.json` |
| 输入无响应 | 是否处于 Play、Game 窗口是否获得焦点、玩家是否死亡或处于房间过渡 |
| 打开后只有素材展示 | 是否打开了 `Assets/Real.unity` |
| 物体变粉 / 摄像机异常 | URP 包是否还原、Graphics/Quality 的渲染管线配置与场景摄像机引用 |
| 角色无法着地 / 墙跳 | Ground、Platform、Wall 层及对应 Collider、LayerMask 配置 |
| 死亡后没有重生 | 当前没有完整自动复活流程；停止 Play 后重新进入测试 |
| 克隆后丢失资源引用 | 对应 `.meta` 是否与资源一起提交，是否发生 GUID 冲突 |

仍无法解决时，用 [Bug 模板](https://github.com/Wangwu-1/-2d-dungeon-rpg/issues/new?template=bug_report.yml)附上复现步骤、Unity 完整版本、提交号和 Console 日志。
