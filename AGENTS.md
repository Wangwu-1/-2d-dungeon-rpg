# Codex 项目工作指南

执行任务前完整阅读本文件。本文依据 GitHub `master` 的当前系统整理，游戏行为以实际源码和场景为准。

## 项目与入口

- Unity **2022.3.62f3c1**，URP **14.0.12**，旧版 Input Manager。
- 当前主场景与构建入口：`Assets/Real.unity`。
- 主要运行时代码位于 `Assets/Script/`，编辑器工具位于 `Assets/Editor/`。
- 玩家与敌人采用 FSM；当前玩家与敌人退出回调均为 `Exit()`。
- 项目已有骷髅、法师、Leviathan、玩家血量事件与血条控制脚本、尖刺、可破坏门、刷怪与房间传送，不能按旧 SampleScene 原型描述项目。

## 改动前分析

任何游戏代码任务，编码前必须先回答：

> 影响哪些系统？是否需要调整现有状态机结构？是否需要新增状态？

分析完成并获得用户确认后再编码。已获得的确认持续有效，不重复请求。文档与仓库配置整理可直接在已授权范围执行。

## 工作流程

1. 阅读 `docs/CODING_RULES.md`；结合本次任务读取实际代码、场景和 `.meta`。
2. 依照确认的范围实现，保持现有状态机模式；不要夹带无关清理。
3. 逐条执行 `docs/REVIEW_CHECKLIST.md` 的适用检查。
4. 每条任务完成后立即追加 `TASK_LOG.txt`；设计或架构决定同步记入 `DecisionLog.md`。
5. 报告实际修改、验证方式与未完成的检查，区分静态检查、Unity 编译、运行和构建。

## 编码约束

- 类 / 方法采用 PascalCase；私有字段 `_camelCase`；公开字段 `camelCase`。
- 新脚本文件与类同名，文件名不含空格；历史文件重命名需单独评估引用。
- Inspector 配置采用 `[Header]` 和 `[SerializeField] private`。
- 新增状态实现 `Enter()`、`Update()`、`Exit()`，清楚定义切换条件与退出清理。
- 不新增 `FindObjectOfType`、`GameObject.Find`、`SendMessage`、`BroadcastMessage`、`Invoke`、`InvokeRepeating`、`DontDestroyOnLoad` 或单例模式；组件引用在初始化阶段缓存，禁止在 `Update()` 中调用 `GetComponent`。
- 历史代码中已有不符规范的用法：单独分析迁移，不借文档任务改动游戏行为。
- 通过 Unity 移动或改名资源，保留 `.meta` GUID；禁止无依据删除所谓“旧资源”。

## 日志格式

`TASK_LOG.txt` 每条记录：

```text
========================================
[YYYY-MM-DD 阶段描述]
指令：[用户指令]
操作：[实际执行的操作]
状态：完成 / 进行中 / 阻塞
备注：[关键信息、实际验证与剩余限制]
```

`DecisionLog.md` 每项设计 / 架构决定：

```text
**[类型] 决定描述**
原因：...
```

类型使用 `[Gameplay]`、`[Architecture]`、`[Balance]`、`[UI]` 或 `[Tech]`。

## 资料索引

- `README.md`：项目首页与操作。
- `docs/GETTING_STARTED.md`：环境、运行和构建。
- `docs/PROJECT_STATUS.md`：当前实现、边界与后续建议。
- `PROJECT_REFERENCE.md`：详细架构与历史记录，可能包含过时描述和本机路径。
- `CONTRIBUTING.md`：贡献流程。
- `docs/THIRD_PARTY_ASSETS.md`：素材说明索引。
