---
name: ui-hud-patterns
description: Structure HUD and menu implementation so UI remains understandable, maintainable, and aligned with UX goals.
origin: everything-game-dev-code
category: engineering-common
---

# UI Hud Patterns

## Purpose
Structure HUD and menu implementation so UI remains understandable, maintainable, and aligned with UX goals.

## Use When
- HUD complexity is growing
- UI logic is leaking into gameplay code
- navigation and state are hard to maintain

## Inputs
- flow maps
- screen inventory
- input model
- localization and accessibility needs

## Process
1. separate state, navigation, and presentation
2. define UI ownership and update triggers
3. minimize coupling between gameplay systems and widget hierarchies
4. support localization, scaling, and platform variation
5. verify focus, persistence, and feedback loops

## Outputs
- UI architecture notes
- HUD ownership map
- navigation model
- integration constraints

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
- ui-ux-designer
- accessibility-reviewer

## Related Commands
- ui-flow-review
- tech-design
- verify

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
