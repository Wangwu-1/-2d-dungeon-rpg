# 开发日志

## 2026-06-13 — 加了单向平台（可以跳穿的地面）

想加一种从下面能跳上去、站在上面又不会掉下来的地面。Unity 自带的 `PlatformEffector2D` 管物理碰撞的部分，但玩家的地面检测是用的 `Physics2D.Raycast`，`PlatformEffector2D` 拦不住射线。

### 干了啥

- 在 Layer 7 加了 `Platform` 层
- 改了 `Player.cs` 的 `IsGroundDetected()`：现在不光检查 Ground 层，也检查 Platform 层。如果射线打中的是 Platform，多加两个判断——脚底在平台表面 **上方**，并且人 **没在上升**，才算落地。这样跳穿的时候不会中途被吸住
- 新建了一个 Platform Tilemap，挂上 `TilemapCollider2D` + `PlatformEffector2D`（打开 Use One Way 和 Use One Way Grouping，Surface Arc 设 180）

### 注意点

- `PlatformEffector2D` 和 `CompositeCollider2D` 不兼容，所以 Platform Tilemap 的「Used By Composite」要关掉
- 如果项目里没有 Platform 层也不会炸，`IsGroundDetected()` 会自动回退到只查原来的 Ground 层

### 手动操作（Unity Editor）

在 Grid 下新建 Platform Tilemap → 设 Layer = Platform → 加 TilemapCollider2D 和 PlatformEffector2D → 用 Tile Palette 在上面画平台瓷砖

---

## 2026-06-03 — 架构大扫除

### 修的 bug

1. **[SerializeField] 写在普通类上根本没生效**：`PlayerPrimaryAttack`、`SkeletonAttackState`、`PlayerHitState` 这些继承的不是 MonoBehaviour，`[SerializeField]` 写了也白写，Inspector 里调了半天其实都是默认值。把攻击半径、击退力度之类的东西挪到父级 MonoBehaviour 上去了

2. **残影每次 new GameObject**：Dash 的时候每 3 帧就 `new GameObject("DashGhost")`，用完 0.3 秒再 Destroy。冲一次搞出五六个对象，GC 吃不消。加了个简单的对象池复用

3. **`Exist()` 应该是 `Exit()`**：全员拼写错误，改过来了

4. **`PlayerHitState.Enter()` 没调 `base.Enter()`**：自己手写了 rb/triggerCalled/timer 的初始化，忘了调父类。因为 `animBoolName` 传的是空字符串所以没触发 bug，但这是个坑

---

## 2026-06-16 — 可破坏 Tilemap 门

想做个像空洞骑士那种砍两下就碎掉的门。

### 干了啥

- **新建 `Assets/Script/Interactables/BreakableDoorTilemap.cs`**：挂在 Tilemap 上，每块 tile 独立受击计数。攻击区域内的所有 tile 一起判定（用 Vector2.Distance 算 tile 中心点在不在攻击圆内），第 1 击红闪，第 2 击 `SetTile(null)` 移除
- **新建 `Assets/Editor/BreakableDoorTilemapSetup.cs`**：Tools 菜单一键创建门 Tilemap
- **改 `Player Primary Attack.cs`**：`ApplyAttackDamage()` 里追加 `BreakableDoorTilemap` 检测（攻击时传入圆形区域）
- **闪现用 Sprite 覆盖层**：因为 Tilemap.SetColor 在 URP 下可能不生效，加了个临时 SpriteRenderer 覆盖层确保红闪在任何管线都看得见

### 手动操作（Unity Editor）

Tools → Setup Breakable Door Tilemap → 用 Tile Palette 在 BreakableDoor 层上画门瓷砖

### 注意点

- 用了 Default 层，没用 Wall 层——否则靠近门会触发壁滑
- 第 1 击后可选择替换为裂纹瓦片（`crackedTile` 字段）

---

## 2026-06-16 — 房间切换系统（空洞骑士风格黑屏过渡）

玩家走到房间出口时屏幕渐黑，出现在下一房间入口，再渐亮。

### 干了啥

- **新建 `Assets/Script/GameManager/RoomManager.cs`**：单例，自动创建全屏黑色 Fade Canvas（动态生成，无需预制体）。`TransitionTo(spawnPoint)` 完整流程：冻结玩家 → 淡出 → 传送 → ForceCameraPosition 瞬移摄像机 → 等待 0.1s → 淡入 → 解冻
- **新建 `Assets/Script/GameManager/RoomExit.cs`**：挂在出口 Trigger 上，`spawnPoint` 字段拖拽指向目标房间出生点
- **改 `Player.cs`**：加 `isTransitioning` 标志
- **改 `Player states.cs`**：Update 开头 early-return，过渡期间冻结所有输入和动画

### 手动操作（Unity Editor）

1. 每个房间出口放一个 GameObject → BoxCollider2D (Is Trigger) → 挂 RoomExit 脚本
2. 每个房间入口放一个空 GameObject 做 spawnPoint
3. 把 RoomExit 的 spawnPoint 拖到目标房间的 spawnPoint

### 注意点

- 可重复使用，走回来再去不会有问题（RoomExit 去掉了 triggerOnce 限制，用 `player.isTransitioning` 防重复触发）
- 不需要往场景里放 RoomManager 对象，第一次调用 TransitionTo 时会自动创建

### 有待优化

- 所有数值（移速、跳跃力、攻击力）硬写在代码里，没做成可配的数据资产
- `Player.cs` 管得太多：移动、跳跃、冲刺、血量、受击、震屏、状态机、输入检测全塞一个文件里
