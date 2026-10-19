# 开发状态与路线

[返回首页](../README.md)

最近文档核对：**2026-10-19**。基于远程 `master` 提交 [`269301a`](https://github.com/Wangwu-1/-2d-dungeon-rpg/commit/269301a471223ec3ff4817809751fc6c17679047) 的源码、包配置和构建入口；功能是否在目标平台正常运行仍需 Unity 验收。

## 源码中已有的系统

| 系统 | 当前实现 | 主要入口 |
| --- | --- | --- |
| 玩家移动 | 指数平滑移动、跳跃、冲刺、墙滑、墙跳 | [Player.cs](../Assets/Script/Player/Player.cs) |
| 近战与受击 | 三段连击、范围命中、击退、短暂无敌、命中停顿 | [Player Primary Attack.cs](../Assets/Script/Player/Player%20Primary%20Attack.cs) |
| 骷髅 / 法师 | 巡逻、追击、攻击、受击、死亡状态 | [Enemy 目录](../Assets/Script/Enemy/) |
| Leviathan Boss | 独立待机、攻击、受击、死亡状态 | [LeviathanEnemy.cs](../Assets/Script/Enemy/LeviathanEnemy.cs) |
| 玩家血条脚本 | 订阅生命变化事件，更新 Slider；入口场景未见该脚本挂载 | [HealthBarController.cs](../Assets/Script/UI/HealthBarController.cs) |
| 房间切换 | 出口触发、画面淡入淡出、玩家传送与摄像机处理 | [RoomManager.cs](../Assets/Script/GameManager/RoomManager.cs) |
| 刷怪 | 距离驱动生成与回收、死亡事件跟踪 | [EnemySpawner.cs](../Assets/Script/Spawner/EnemySpawner.cs) |
| 尖刺 / 可破坏门 | 尖刺扣血并回传安全点、逐块破坏门瓦片 | [Hazards](../Assets/Script/Hazards/) / [Interactables](../Assets/Script/Interactables/) |
| 视觉反馈 | 冲刺残影、受击闪烁、背景视差与震屏 | [Effects 目录](../Assets/Script/Effects/) |

## 当前边界

- **死亡后的流程尚未闭环**：玩家死亡会停止速度、重力与组件更新；尚无完整死亡动画状态、自动复活与 Game Over 交互。
- **法师与 Boss 当前使用近战范围伤害**：不能把角色名称理解为已实现远程法术或复杂多阶段 Boss 战。
- **房间传送已有实现**：它是在场景内传送至配置的入口，未据此宣称已具备通关结算或程序化地牢。
- **血条脚本不等于完整 HUD**：已有玩家血量事件和 Slider 控制器，本次静态检查未在 `Real.unity` 发现控制器挂载；未见敌人血条系统，UI 接入仍需运行验收。
- **击杀后的重刷逻辑需修正**：当前生成器将死亡标记保持为真，击杀后离开再进入不会自动刷新；活敌远离回收后可再次生成。这与历史参考文档的描述有差异。
- **文档存在历史差异**：[PROJECT_REFERENCE.md](../PROJECT_REFERENCE.md) 保留详细开发记录，其中的本机路径、数值和早期说明需结合当前源码核对。
- **自动化仍需补充**：现有构建脚本没有显式失败退出码；本次不新增未验证的 CI 工作流，也不将静态审查写成运行测试通过。

## 建议开发顺序

以下是后续任务建议，尚未实现为本次变更。

1. **完整的游玩循环**：确定玩家死亡状态、复活点、重试与游戏结束交互，验证死亡与房间切换的边界。
2. **战斗与关卡回归**：逐场景检查连击判定、尖刺安全点、门破坏、敌人刷新及 Boss 的生命周期。
3. **敌人和反馈扩展**：法师投射物、Boss 招式与阶段、敌人血条、音效与音乐。
4. **工程与发布流程**：统一历史命名、梳理依赖和素材来源、完善构建失败检测，再按需要引入自动化验证。
5. **更长线的 RPG 系统**：背包、存档、菜单和关卡扩展，逐项确认范围后开发。

每项游戏代码任务开始前，先说明会影响哪些系统、状态机是否调整及是否新增状态。设计决定写入 [DecisionLog.md](../DecisionLog.md)，实施与实际验证记录到 [TASK_LOG.txt](../TASK_LOG.txt)。
