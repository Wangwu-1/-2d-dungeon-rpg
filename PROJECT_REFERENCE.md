# 2D 地牢横版 RPG — 项目架构参考

> 每次新增/修改系统时更新本文档。
> 保持与代码实际状态一致。

---

## 1. 总览

- **引擎：** Unity 2022.3.62f3c1, URP 14
- **项目路径：** `C:\Users\ethan\Desktop\Gaming_Project\1\2D dungeon-style side-scrolling RPG\`
- **主场景：** `Assets/Real.unity`
- **构建脚本：** `Assets/Editor/BuildScript.cs`

---

## 2. 玩家系统

### 2.1 玩家脚本

| 文件 | 职责 |
|------|------|
| `Assets/Script/Player/Player.cs` | 主类，单例 `Player.Instance`，状态机+属性+生命+伤害 |
| `Assets/Script/Player/Player States Machine.cs` | 状态机 (`Initialize`, `ChangeStates`) |
| `Assets/Script/Player/Player states.cs` | 状态基类 |
| `Assets/Script/Player/Player Animation Triggers.cs` | 接收动画事件转发到当前状态 |

### 2.2 状态列表

| 状态 | 文件 | 动画参数 | 描述 |
|------|------|----------|------|
| Idle | `Player idle States.cs` | "Idle" | 站立，有输入→Move |
| Move | `Player Movement States.cs` | "Move" | 水平移动 |
| Jump | `Player Jump States.cs` | "Jump" | 跳跃，变高度（松 Space 减半速） |
| Air | `Player Air States.cs` | "Jump" | 空中水平控制（0.8x），检测WallSlide |
| Dash | `Player Dash States.cs` | "Dash" | 冲刺（LeftShift），残影特效 |
| WallSlide | `Player WallSlide States.cs` | "Wallslide" | 贴墙减速下落，Space→WallJump |
| WallJump | `Player WallJump States.cs` | "Jump" | 蹬墙跳远离墙 |
| PrimaryAttack | `Player Primary Attack.cs` | "Attack" | 3段连击，OverlapCircle检测 |
| Hit | `PlayerHitState.cs` | ""（空） | 受击+击退，红闪0.3s |

### 2.3 核心字段

```csharp
// 移动
MoveSpeed = 10, Jumpforce = 25, GravityScale = 5
DashSpeed = 15, DashDuration = 0.3s, DashCooldown = 0.6s
wallJumpForceX = 5

// 生命
maxHealth = 5, invincibleDuration = 0.5s

// 击退
knockbackForceX = 5, knockbackForceY = 3

// 攻击
attackRadius = 2.5f, 3段连击窗口 2s
```

### 2.4 碰撞检测

**地面检测 `IsGroundDetected()`：** Unity 碰撞回调 (`OnCollisionEnter2D` / `OnCollisionExit2D`)
- 用 HashSet 追踪当前接触的 Collider
- 检测 Layer: Ground(Layer3) + Platform(Layer7)
- 需要 `contact.normal.y > 0.5`（脚底踩到才计数）
- Platform 额外条件：非上升状态（`rb.velocity.y <= 0.05f`）

**墙检测 `IsWallDetected()`：** Physics2D.Raycast 从 CapsuleCollider2D 边缘水平
- 检测 Layer: Wall(Layer6)

**攻击检测：** Physics2D.OverlapCircleAll，偏移 `Facedir * 1.2`，半径 `attackRadius`

**注意：** 项目不使用 OnTriggerEnter2D（尖刺系统除外）

### 2.5 伤害系统

`Player.TakeDamage(float damage, float knockbackDirX)`:
1. 检查无敌/死亡
2. 扣血 + Cinemachine Impulse 震屏
3. 如果死亡：velocity归零, gravityScale=0, 禁用Player组件
4. 否则：记录击退方向 → 0.5s无敌 → 切到 HitState

`Player.TakeSpikeDamage(float damage)`（尖刺专用）:
1. 与 TakeDamage 相同的前置检查
2. 扣血 + 震屏
3. 如果死亡：死在原地
4. 否则：传回 `_lastSafePosition` → velocity归零 → 标记 `lastHitKnockbackDirX=0` → 无敌 → HitState

### 2.6 安全位置追踪（尖刺系统）

```csharp
private Vector2 _lastSafePosition;
```
- `Start()` 初始化为 `transform.position`
- 每帧 `TrackSafePosition()`：在地面 + 非无敌 + 未死亡 时更新
- 无敌期间不更新，防止传送后立即锁定不安全位置

### 2.7 翻滚/Sprite方向

- 使用 `transform.Rotate(0,180,0)` 翻转（非 SpriteRenderer.flipX）
- `Facedir`：1=右, -1=左
- `FlipController(x)`：根据 rb.velocity.x 自动翻转

---

## 3.敌人系统

### 3.1 基类与架构

| 文件 | 职责 |
|------|------|
| `Assets/Script/Enemy/Enemy.cs` | 基类：移动、检测、碰撞、属性 |
| `Assets/Script/Enemy/EnemyState.cs` | 敌人状态基类 |
| `Assets/Script/Enemy/EnemyStateMachine.cs` | 敌人状态机 |
| `Assets/Script/Enemy/SkeletonEnemy.cs` | 骷髅敌人具体实现 |

### 3.2 骷髅状态

| 状态 | 文件 | 说明 |
|------|------|------|
| Idle | `SkeletonIdleState.cs` | 等待1.5s，检测到玩家→Battle |
| Move | `SkeletonMoveState.cs` | 巡逻，墙/边缘→Flip |
| Battle | `SkeletonBattleState.cs` | 追玩家(1.3x 速度)，攻击范围→Attack |
| Attack | `SkeletonAttackState.cs` | 0.7s，0.4s时触发伤害(OverlapCircle) |
| Hit | `SkeletonHitState.cs` | 0.5s眩晕+击退，红色闪光 |
| Death | `SkeletonDeathState.cs` | 播放死亡动画→1s后→销毁 |

### 3.3 敌人检测

- `IsPlayerDetected()`：距离检测
- `IsPlayerInAttackRange()`：水平+垂直范围检测
- 碰撞：Raycast 检测地面/墙体

### 3.4 敌人造成伤害

`SkeletonAttackState.ApplyAttackDamage()`:
Physics2D.OverlapCircleAll → 检测 Tag "Player" → 调用 `player.TakeDamage(damage, skeleton.facingDir)`

---

## 4. 尖刺系统

### 4.1 文件

| 文件 | 职责 |
|------|------|
| `Assets/Script/Hazards/Spike.cs` | 触发器脚本：OnTriggerEnter2D + OnTriggerStay2D |
| `Assets/Editor/SpikeTilemapSetup.cs` | Editor工具：Tools → Setup Spike Tilemap |

### 4.2 层

- **Spike (Layer 8)** — 触发检测专用层
- 碰撞矩阵：全碰撞（默认），不影响地面/墙体检测

### 4.3 工作流程

1. 玩家进入 Spike Trigger
2. `Spike.cs` 检测到 Player 组件
3. 调用 `Player.TakeSpikeDamage(damage)`
4. 传回 `_lastSafePosition` + 无敌 + HitState
5. HitState 中 `dir=0` 跳过推送

### 4.4 场景布置

- Tilemap 方式：Grid 下建 "Spikes" tilemap
- TilemapCollider2D 必须设 `Is Trigger = true`
- 也可用独立 GameObject + BoxCollider2D（Is Trigger）

---

## 5. 刷怪系统

### 5.1 文件

| 文件 | 职责 |
|------|------|
| `Assets/Script/Spawner/EnemySpawner.cs` | 根据玩家距离生成/回收敌人 |
| `Assets/Script/Enemy/Enemy.cs` | 基类（新增 `OnDied` 事件供 Spawner 监听） |
| `Assets/Editor/SkeletonPrefabCreator.cs` | Editor工具：Tools → Create Enemy Prefabs |

### 5.2 架构

```
EnemySpawner (场景中放置)
  ├── enemyPrefab → 引用 Prefab
  ├── spawnRadius (15)  → 玩家进入此范围时生成
  ├── despawnRadius (20) → 玩家超出此范围时回收
  │
  └── 生命周期
       ├── Start() → 玩家可能在范围内，立即尝试生成
       ├── Update() → 每帧检测距离，生成或回收
       └── OnDestroy() → 断开事件连接
```

### 5.3 事件流

```
EnemySpawner                                 Enemy (Enemy.cs)
    │                                            │
    │── SpawnEnemy()                             │
    │    Instantiate(prefab) ──────────────────→  │
    │    Subscribe OnDied                         │
    │                                            │
    │                              (玩家杀死敌人)  │
    │                                            │── Die()
    │                                                OnDied?.Invoke()
    │←────────────────────────────────────────── ──┘
    │  OnEnemyDied()                                (OnDied = null)
    │    _enemyDead = true
    │    (尸体会播完死亡动画后自毁 DestroySelf)
    │
    │  (玩家离开范围 → _enemyDead 保持 true)
    │  (玩家再进入范围 → 重置死亡标记，重新生成)
    │
    │  (玩家离开范围而敌人还活着 → 立即 Destroy)
    │    DespawnEnemy()
    │    _enemyDead = false（下次进范围刷新）
```

### 5.4 工作流程

1. **首次进入范围** → `SpawnEnemy()` 实例化 Prefab
2. **战斗中** → 敌人正常巡逻/追逐/攻击
3. **敌人死亡** → `OnDied` 通知 Spawner，Spawner 标记已死
4. **玩家离开** → `_enemyDead = true`，不生成（留清白在人间）
5. **玩家再进** → `_enemyDead` 还是 true？重置，刷新的敌人出来！
6. **玩家离开 + 敌人活着** → `DespawnEnemy()` 直接 Destroy，省性能

### 5.5 场景布置步骤

1. 在 Unity 中点 **Tools → Create Enemy Prefabs**
   - 自动把场景中的 Skeleton 存为 `Assets/Prefabs/Enemies/Skeleton.prefab`
   - 原地替换为 `EnemySpawner`，已绑定好 Prefab 引用
2. 调整 Spawner 的 `spawnRadius` / `despawnRadius` 参数
3. 复制 Spawner 到需要刷怪的其它位置
4. 删掉场景中多余的旧 Skeleton

---



## 6. 可交互物

### 6.1 可破坏 Tilemap 门

| 文件 | 职责 |
|------|------|
| `Assets/Script/Interactables/BreakableDoorTilemap.cs` | 挂在 Tilemap 上，每块 tile 受击2次消失 |
| `Assets/Editor/BreakableDoorTilemapSetup.cs` | Editor工具：Tools → Setup Breakable Door Tilemap |

**原理：**
- `HitArea(Vector3 center, float radius)` 扫描圆形区域内所有 tile，独立受击计数
- 第1击：Tile 红闪（Sprite 覆盖层 + SetColor 双重保障，兼容 URP）
- 第2击：`tilemap.SetTile(null)` 移除，TilemapCollider2D 自动更新物理碰撞形状
- 攻击判断：从 `attackPos` 传入玩家攻击圆的中心和半径

**注意事项：**
- 使用 Default 层（不用 Wall 层，否则触发壁滑）
- 第1击后可选择替换为裂纹瓦片（`crackedTile` 字段）

**场景布置：**
1. `Tools → Setup Breakable Door Tilemap` 创建 Tilemap
2. 从资源图创建 Tile Asset（用 `wood_env.png` 或 `castle_env.png` 切片）
3. 用 Tile Palette 在 BreakableDoor 层上刷门瓷砖

---

## 7. 视觉效果

| 文件 | 说明 |
|------|------|
| `Assets/Script/Effects/GhostTrail.cs` | Dash 残影，对象池 |
| `Assets/Script/Effects/DamageFlash.cs` | 受击红色闪烁（敌人用） |
| `Player.cs` → `CinemachineImpulseSource` | 震屏 |

---

## 8. 房间切换系统

### 8.1 文件

| 文件 | 职责 |
|------|------|
| `Assets/Script/GameManager/RoomManager.cs` | 单例，控制黑屏淡入/淡出 + 传送 + 摄像机瞬移 |
| `Assets/Script/GameManager/RoomExit.cs` | 出口 Trigger，绑定 spawnPoint |

### 8.2 工作流程

```
玩家走进 RoomExit Trigger
  → RoomManager.TransitionTo(spawnPoint)
    1. player.isTransitioning = true  (冻结输入)
    2. FadeOut → 画面渐黑 (0.3s)
    3. 传送 player 到 spawnPoint
    4. 摄像机瞬移 (vcam.PreviousStateIsValid = false)
    5. 等待 0.1s
    6. FadeIn → 画面渐亮 (0.3s)
    7. player.isTransitioning = false (解冻)
```

### 8.3 冻结机制

- `Player.isTransitioning` 标志：过渡期间设置
- `Player states.Update()` 早返：过渡期间跳过 Input.GetAxisRaw 和 timers
- 过渡完成后自动恢复

### 8.4 场景布置

1. 每个房间出口：空 GameObject → BoxCollider2D (Is Trigger) → 挂 RoomExit
2. 每个房间入口：空 GameObject 做 spawnPoint
3. RoomExit.spawnPoint 字段拖拽指向目标房间的 spawnPoint
4. 无需手动放 RoomManager 对象（第一次调用 TransitionTo 自动创建）

---

## 9. 摄像机

- **Cinemachine Virtual Camera** + **Framing Transposer**
- 跟随 Player，居中，阻尼 1
- Cinemachine Collider 防止穿墙
- Orthographic Size: 6.1
- 过渡期间用 `vcam.PreviousStateIsValid = false` 实现瞬移

---

## 10. Layer 配置

| 索引 | 名称 | 用途 |
|------|------|------|
| 0 | Default | 玩家、Grid、可破坏门 |
| 3 | Ground | 地面 Tilemap |
| 6 | Wall | 墙壁 Tilemap |
| 7 | Platform | 单向平台 (PlatformEffector2D) |
| **8** | **Spike** | **尖刺触发器** |
| **9** | **Enemy** | **敌人（Skeleton, Wizard）** |

碰撞矩阵：Enemy ↔ Default 和 Enemy ↔ Enemy 已关闭，敌人可与玩家/其他敌人叠站。

---

## 11. 开发日志

### 2026-06-14 — 尖刺陷阱（空洞骑士风格）
**改了啥：**
- 新建 `Assets/Script/Hazards/Spike.cs` — 触发器脚本，玩家碰到就掉血
- 新建 `Assets/Editor/SpikeTilemapSetup.cs` — Unity 菜单点一下自动建尖刺 Tilemap
- `Player.cs` 加了安全位置追踪 + `TakeSpikeDamage()` 专门给尖刺用
- `PlayerHitState.cs` 小改：尖刺伤害不推人（改成传送回安全点）
- `ProjectSettings/TagManager.asset` 加了 Spike 层（Layer 8）

**怎么用：** 在 Unity 里点 `Tools → Setup Spike Tilemap`，然后在地图上画尖刺瓷砖就行。

---

### 2026-06-14 — 刷怪系统（离开房间再进来怪就刷新）
**改了啥：**
- 新建 `EnemySpawner.cs` — 场景里放一个 Spawner 点，它会根据玩家距离自动生怪/收怪
- 新建 `SkeletonPrefabCreator.cs` — 一键把场景里的骷髅做成 Prefab 并替换成 Spawner
- `Enemy.cs` 加了个 `OnDied` 事件，Spawner 监听这个知道怪死了

**怎么用：**
1. 在 Unity 点 `Tools → Create Enemy Prefabs`
2. 场景里的骷髅自动变成 Prefab + Spawner
3. 调一下 Spawner 的范围参数，复制到其他位置

**工作逻辑：**
- 玩家靠近（15m）→ 生成敌人
- 玩家走远（20m）→ 回收敌人（省性能）
- 杀死敌人 → 死亡动画正常播，不重生
- 离开再回来 → 刷个新的出来，满血满状态

---

### 2026-06-14 — 指数加减速（走路手感优化）
**改了啥：**
- `Player.cs` 加了三个参数：`accelerationRate(12)` / `decelerationRate(18)` / `airAccelMultiplier(0.5)`
- `Player.cs` 加了新方法 `SetVelocityXExponential()` — 用指数趋近公式平滑加减速
- `PlayerMovementStates.cs` — 走路改用指数加速，松手时用更高的减速率滑停
- `PlayerAirStates.cs` — 空中移动改用指数加速（受 `airAccelMultiplier` 影响，空中操控更弱）
- `PlayerJumpStates.cs` — 跳起水平移动继承 AirStates，去掉了重复的 velocity 覆盖

**公式：**
```
每帧 vel.x = lerp(vel.x, targetX, 1 - exp(-rate * dt))
```
rate 越大越快到达目标速度。地面加速 12，减速 18（松手停得快），空中乘 0.5（操控感弱）

**怎么调：**
- `accelerationRate` 更高 → 起步更跟手
- `decelerationRate` 更高 → 松手停得更干脆
- `airAccelMultiplier` 更低 → 空中更难变向（更像马里奥/空洞骑士）

---

### 2026-06-14 — 修复空中移动卡顿
**问题：** 空中左右移动时感觉卡卡的，一跳起来转向不跟手。

**根因：** `PlayerAirStates.Exit()` 原来会把水平速度清 0：
```csharp
player.SetVelocity(0, rb.velocity.y);
```
以前用直接设 velocity 的方式清 0 的下一帧就恢复了，感觉不到。但配上**指数加速**后，从 0 加速到目标速度需要好几帧，每次清空都要重新爬，手感就卡了。

两次触发：
1. Jump→Air（跳起到达顶点下落时）→ X 速度被清
2. Air→落地 → X 速度被清，落地后又要从 0 加速

**改了啥：**
- `AirStates.Exit()` 不再设速度，让下一个状态继承水平动量
- `IdleStates.Update()` 加了自然减速（落地后不带输入时会滑停）
- `SetVelocityXExponential()` 加了接近目标时直接归位的保护，防止微漂移

---

### 2026-06-16 — 可破坏 Tilemap 门

**改了啥：**
- 新建 `Assets/Script/Interactables/BreakableDoorTilemap.cs` — Tilemap 门每块 tile 独立受击计数
- 新建 `Assets/Editor/BreakableDoorTilemapSetup.cs` — Tools 菜单一键创建门 Tilemap
- 改 `Player Primary Attack.cs` — `ApplyAttackDamage()` 追加 `BreakableDoorTilemap` 检测

**怎么用：** `Tools → Setup Breakable Door Tilemap` → 刷 Tile Palette

**注意：** 闪现用 Sprite 覆盖层 + SetColor 双重保障，兼容 URP

---

### 2026-06-16 — 房间切换系统（空洞骑士风格黑屏过渡）

**改了啥：**
- 新建 `Assets/Script/GameManager/RoomManager.cs` — 单例，动态创建黑色 Fade Canvas，控制淡入淡出
- 新建 `Assets/Script/GameManager/RoomExit.cs` — 出口 Trigger，绑定 spawnPoint
- 改 `Player.cs` — 加 `isTransitioning` 标志
- 改 `Player states.cs` — Update 早返，过渡期间冻结输入和动画

**怎么用：**
1. 每个房间出口放空 GO → BoxCollider2D (Is Trigger) → 挂 RoomExit
2. 每个房间入口放空 GO 做 spawnPoint
3. RoomExit.spawnPoint 拖到目标房间 spawnPoint
4. 无需手动放 RoomManager（自动创建）

**注意：** 可重复使用，RoomExit 没次数限制，用 `player.isTransitioning` 防重复触发

---

### 2026-06-21 — Wizard 敌人 Prefab

**改了啥：**
- 新建 `Assets/Script/Enemy/WizardEnemy.cs` — 继承 Enemy，6 个状态（Idle/Move/Battle/Attack/Hit/Death）
- 新建 6 个 Wizard 状态类（`WizardIdleState.cs` 等），模式同 Skeleton
- 新建 `Assets/Editor/WizardSetupTool.cs` — Unity 菜单 `Tools → Wizard → Setup Wizard Prefab` 一键完成：
  - 切片 Wizard 八张 Sprite sheet（77×190 帧）
  - 生成动画剪辑（6fps~10fps）
  - 生成 Animator Controller（4 个 Bool 参数：isMoving/isAttacking/isHit/isDead）
  - 生成 `Assets/Prefabs/Enemies/Wizard.prefab`
- 攻击目前使用近战 OverlapCircle（`WizardAttackState.cs` 标记 TODO，后续改为法术投射物）

**怎么用：**
1. 在 Unity 中执行 `Tools → Wizard → Setup Wizard Prefab`
2. 把 `Assets/Prefabs/Enemies/Wizard.prefab` 拖入场景
3. 或通过 EnemySpawner 的 `enemyPrefab` 字段引用 Wizard.prefab 实现动态刷怪

---

### 2026-06-21 — 敌人独立层（可叠站）

**问题：** 玩家和敌人、敌人在同层（Default），物理碰撞把敌人互相挤开，没法叠着站，打群架时会被推开。

**改了啥：**
- `ProjectSettings/TagManager.asset` — 新增 **Enemy** 层（Layer 9）
- `Enemy.cs` — `Start()` 时递归把自身和所有子对象设为 Enemy 层；调用 `Physics2D.IgnoreLayerCollision(Enemy, Default)` 和 `IgnoreLayerCollision(Enemy, Enemy)` 关闭物理碰撞
- `Player Primary Attack.cs` — `OverlapCircleAll` 加 `LayerMask.GetMask("Default", "Enemy")` 确保仍能命中敌人
- `SkeletonAttackState.cs` — 加 `LayerMask.GetMask("Default")`
- `WizardAttackState.cs` — 同上

**效果：**
- 敌人可互相叠加，玩家可穿过敌人
- 所有攻击检测正常（OverlapCircle 不受 IgnoreLayerCollision 影响）
- 敌人自身检测（地面/墙壁 Raycast）使用显式 LayerMask，不受影响

---

### 2026-06-21 — Wizard 轻重攻击（25% 重击概率）

**改了啥：**
- `WizardAttackState.cs` — `Enter()` 时 `Random.value < 0.25f` 决定本次攻击轻重
  - 普通攻击 (Attack1)：24帧动画，伤害倍率 ×1，半径不变，击退方向 ×1
  - 重攻击 (Attack2)：21帧动画，伤害倍率 ×2，半径 ×1.3，击退方向 ×1.5
- `WizardEnemy.cs` — 新增 `heavyAttackDamageMultiplier(2f)` 和 `heavyAttackRadiusMultiplier(1.3f)`
- `WizardSetupTool.cs` — 新增 Attack2 动画剪辑 (`Wizard_attack2`, 21帧, 10fps)
- Animator Controller 新增 `isHeavyAttack` bool 参数 + `Wizard_attack2` 状态
  - AnyState 跳转优先级：`isDead > isHit > isAttacking+isHeavyAttack(Attack2) > isAttacking(Attack1)`
- `Exit()` 时清除 `isHeavyAttack` 防止污染下一状态

---

### 2026-06-22 — Parallax 视差效果 + 修复移动抽搐 Idle bug

**改了啥：**
- 新建 `Assets/Script/Effects/ParallaxEffect.cs` — 通用视差组件
  - `parallaxFactor` (0~1)：0=贴屏幕不动（无限远），1=随世界不视差
  - `horizontalOnly`：侧滚式锁定垂直视差
  - `LateUpdate` 自动追踪 `Camera.main` 偏移
  - 使用 `cameraStartPosition` 计算偏移，修复上一版错误比较相机与图层位置的问题

**修复 bug：** 移动时突然抽搐到 Idle 一帧
- **根因：** `PlayerAirStates` 落地时固定 `ChangeStates(idleStates)`，若地面检测短暂丢失再恢复，走 `Move→Air→Idle→Move` 链，Idle 那帧可见
- **修复：** `PlayerAirStates` 落地时检查 `Xinput != 0`，有输入直接切 `Movementstates`，无输入才切 `idleStates`

**视觉章节更新：**
| 文件 | 说明 |
|------|------|
| `Assets/Script/Effects/ParallaxEffect.cs` | 视差效果，挂到 Sprite 上配置 factor 即可 |
