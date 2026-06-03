---
name: input-abstraction
description: Design input around player intent and supported device families instead of hardcoding device-specific behavior everywhere.
origin: everything-game-dev-code
category: engineering-common
---

# Input Abstraction

## Purpose
Design input around player intent and supported device families instead of hardcoding device-specific behavior everywhere.

## Use When
- multiple input devices are supported
- rebinds or accessibility matter
- UI and gameplay input are becoming tangled

## Inputs
- action list
- supported devices
- UI flow
- accessibility requirements

## Process
1. define actions by intent
2. separate raw device input from gameplay/UI action handling
3. document action states and context switching
4. support rebinding and persistence where needed
5. test edge cases around focus, pause, and overlays

## Outputs
- action map
- context model
- device support notes
- rebinding requirements

## Quality Bar
- makes ownership, state flow, and failure behavior explicit
- improves maintainability without over-abstracting
- supports testing, debugging, and safe iteration

## Common Failure Modes
- coupling systems through hidden globals or timing assumptions
- writing logic that is hard to test or debug
- optimizing the wrong layer before measuring

## Related Agents
- ui-programmer
- accessibility-reviewer
- gameplay-programmer

## Related Commands
- ui-flow-review
- verify
- tech-design

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
