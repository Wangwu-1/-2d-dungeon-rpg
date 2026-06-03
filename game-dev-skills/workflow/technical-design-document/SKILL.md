---
name: technical-design-document
description: Turn feature intent into a Technical Design Document that is implementable, reviewable, and testable.
origin: everything-game-dev-code
category: workflow
---

# Technical Design Document

## Purpose
Turn feature intent into a Technical Design Document that is implementable, reviewable, and testable.

## Use When
- a feature has non-trivial architecture or integration risk
- multiple systems need stable interfaces
- save/load, networking, tooling, or migration impact exists

## Inputs
- approved design intent
- project constraints
- engine constraints
- testing and performance expectations

## Process
1. define goals, non-goals, constraints, and assumptions
2. map runtime architecture, state ownership, and interfaces
3. document failure modes, migration concerns, and rollout strategy
4. capture testing, telemetry, and performance impact
5. highlight decisions that should become ADRs

## Outputs
- technical design document
- interface and ownership notes
- risk list
- test and rollout considerations

## Quality Bar
- produces a current source of truth, not disconnected notes
- names owners, risks, and next actions explicitly
- separates decisions from assumptions and open questions

## Common Failure Modes
- outdated docs that no longer match reality
- plans with no owner or no exit criteria
- hiding risks until they become schedule blockers

## Related Agents
- technical-design-lead
- architect
- gameplay-programmer

## Related Commands
- tech-design
- plan
- verify

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
