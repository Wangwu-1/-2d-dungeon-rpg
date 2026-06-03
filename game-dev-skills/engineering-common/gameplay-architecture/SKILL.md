---
name: gameplay-architecture
description: Define system ownership, boundaries, state flow, and extensibility for gameplay code across engines.
origin: everything-game-dev-code
category: engineering-common
---

# Gameplay Architecture

## Purpose
Define system ownership, boundaries, state flow, and extensibility for gameplay code across engines.

## Use When
- a feature touches multiple systems
- architecture is becoming hard to reason about
- new systems need stable contracts

## Inputs
- feature intent
- engine constraints
- existing architecture
- test and performance requirements

## Process
1. define system responsibilities and boundaries
2. map state ownership and event flow
3. separate authored data, runtime logic, and presentation
4. identify extension points and anti-coupling measures
5. record risks, migration concerns, and validation strategy

## Outputs
- architecture sketch
- system boundary notes
- state ownership map
- implementation constraints

## Quality Bar
- makes ownership, state flow, and failure behavior explicit
- improves maintainability without over-abstracting
- supports testing, debugging, and safe iteration

## Common Failure Modes
- coupling systems through hidden globals or timing assumptions
- writing logic that is hard to test or debug
- optimizing the wrong layer before measuring

## Related Agents
- architect
- technical-design-lead
- gameplay-programmer

## Related Commands
- tech-design
- plan
- verify

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
