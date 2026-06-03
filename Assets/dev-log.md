# 开发日志

## 2026-06-03 — 代码架构检查 & 修复

### 检查工具
`unity-gameplay-patterns` skill + 人工审查

### 发现的问题

#### 🔴 严重

1. **[SerializeField] 写在非 MonoBehaviour 类上**
   - `PlayerPrimaryAttack.attackRadius`、`SkeletonAttackState.attackRadius`、`PlayerHitState.knockbackForceX/Y`
   - 这些类继承普通 C# 类而非 MonoBehaviour，[SerializeField] 完全不生效
   - 数值永远取默认值，无法在 Inspector 中调整
   - **修复**: 把攻击半径/击退力度移到父级 MonoBehaviour（Player/SkeletonEnemy）的 public 字段，状态类通过引用读取

2. **残影无对象池**
   - `PlayerDashStates` 每 3 帧 `new GameObject("DashGhost")`，`GhostTrail` 在 0.3s 后 Destroy
   - 冲刺一次产生约 6 个对象，频繁 GC 分配
   - **修复**: 添加简单对象池复用残影

#### 🟡 一般

3. **Player 状态方法名 typo**: `Exist()` 应为 `Exit()` — 全项目 Player 侧统一写错，已修正

4. **PlayerHitState.Enter() 跳过了 base.Enter()**:
   - 自己实现了 rb/triggerCalled/statestime 但没调 `base.Enter()`
   - 因 animBoolName 传空字符串未触发 bug，但脆弱

5. **数据散落在代码中，无可配数据资产** (待后续优化)
   - 所有数值（移速、跳跃力、攻击力等）硬编码为 public 字段
   - 未使用 ScriptableObject 做角色配置

6. **Player.cs 职责过重** (待后续优化)
   - 同时负责：移动、跳跃、冲刺、血量、受击、震屏、状态机、输入检测
