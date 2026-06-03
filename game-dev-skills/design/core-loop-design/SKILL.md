---
name: core-loop-design
description: Define the primary player activity loop and the support loops that sustain mastery, variety, and progression.
origin: everything-game-dev-code
category: design
---

# Core Loop Design

## Purpose
Define the primary player activity loop and the support loops that sustain mastery, variety, and progression.

## Use When
- a concept needs gameplay structure
- the current loop feels unfocused
- new systems risk diluting the core fantasy

## Inputs
- product pillars
- target audience
- competitive references
- session and progression goals

## Process
1. state what the player repeatedly does and why it is satisfying
2. map supporting loops that reinforce the core loop
3. identify inputs, feedback, rewards, and failure points
4. test whether the loop fits the intended session length and mastery curve
5. cut systems that do not serve the loop

## Outputs
- core loop definition
- support loop map
- design constraints
- candidate tutorial beats

## Quality Bar
- supports the core fantasy and player goals
- defines readable rules, edge cases, and feedback
- creates concrete hooks for tuning, telemetry, and QA

## Common Failure Modes
- adding systems that do not serve the core loop
- shipping vague rules that QA and engineering must guess at
- tuning without instrumentation or hypotheses

## Related Agents
- systems-designer
- gdd-designer
- level-designer

## Related Commands
- gdd
- plan
- onboarding

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
