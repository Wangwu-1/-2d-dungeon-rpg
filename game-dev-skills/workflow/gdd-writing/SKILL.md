---
name: gdd-writing
description: Create or update a Game Design Document that is actionable for design, engineering, QA, production, and content teams.
origin: everything-game-dev-code
category: workflow
---

# GDD Writing

## Purpose
Create or update a Game Design Document that is actionable for design, engineering, QA, production, and content teams.

## Use When
- starting a new game or major feature
- the current design intent is fragmented across chats and tickets
- systems, loops, or content rules need a durable source of truth

## Inputs
- product pillars and target audience
- core loop assumptions
- feature list and scope constraints
- known technical or production constraints

## Process
1. define the player promise, pillars, and target audience
2. describe core and supporting loops with clear rules
3. document feature behavior, progression, UI, content, and edge cases
4. separate goals, assumptions, decisions, and open questions
5. hand off sections that need technical, QA, or production follow-up

## Outputs
- current GDD
- feature sections with acceptance criteria
- open questions list
- cross-discipline follow-up items

## Quality Bar
- produces a current source of truth, not disconnected notes
- names owners, risks, and next actions explicitly
- separates decisions from assumptions and open questions

## Common Failure Modes
- outdated docs that no longer match reality
- plans with no owner or no exit criteria
- hiding risks until they become schedule blockers

## Related Agents
- gdd-designer
- planner
- systems-designer
- producer

## Related Commands
- gdd
- plan
- update-docs

## Notes
- Keep this skill aligned with the relevant rules layer and current project documentation.
- If engine-specific constraints materially change the workflow, hand off to the matching engine skill or engine-specific reviewer.
