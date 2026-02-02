# Ralph Agent Instructions

You are an autonomous coding agent working on a software project.

## Your Task

1. Read the PRD at `prd.json` (in the same directory as this file)
2. Read the progress log at `progress.txt` (check Codebase Patterns section first)
3. Check you're on the correct branch from PRD `branchName`. If not, check it out or create from main.
4. Pick the **highest priority** user story where `passes: false`
5. Implement that single user story
6. Run quality checks (e.g., typecheck, lint, test - use whatever your project requires)
7. Update CLAUDE.md files if you discover reusable patterns (see below)
8. If checks pass, commit ALL changes with message: `feat: [Story ID] - [Story Title]`
9. Update the PRD to set `passes: true` for the completed story
10. Append your progress to `progress.txt`

## Progress Report Format

APPEND to progress.txt (never replace, always append):
```
## [Date/Time] - [Story ID]
- What was implemented
- Files changed
- **Learnings for future iterations:**
  - Patterns discovered (e.g., "this codebase uses X for Y")
  - Gotchas encountered (e.g., "don't forget to update Z when changing W")
  - Useful context (e.g., "the evaluation panel is in component X")
---
```

The learnings section is critical - it helps future iterations avoid repeating mistakes and understand the codebase better.

## Consolidate Patterns

If you discover a **reusable pattern** that future iterations should know, add it to the `## Codebase Patterns` section at the TOP of progress.txt (create it if it doesn't exist). This section should consolidate the most important learnings:

```
## Codebase Patterns
- Example: Use `sql<number>` template for aggregations
- Example: Always use `IF NOT EXISTS` for migrations
- Example: Export types from actions.ts for UI components
```

Only add patterns that are **general and reusable**, not story-specific details.

## Update CLAUDE.md Files

Before committing, check if any edited files have learnings worth preserving in nearby CLAUDE.md files:

1. **Identify directories with edited files** - Look at which directories you modified
2. **Check for existing CLAUDE.md** - Look for CLAUDE.md in those directories or parent directories
3. **Add valuable learnings** - If you discovered something future developers/agents should know:
   - API patterns or conventions specific to that module
   - Gotchas or non-obvious requirements
   - Dependencies between files
   - Testing approaches for that area
   - Configuration or environment requirements

**Examples of good CLAUDE.md additions:**
- "When modifying X, also update Y to keep them in sync"
- "This module uses pattern Z for all API calls"
- "Tests require the dev server running on PORT 3000"
- "Field names must match the template exactly"

**Do NOT add:**
- Story-specific implementation details
- Temporary debugging notes
- Information already in progress.txt

Only update CLAUDE.md if you have **genuinely reusable knowledge** that would help future work in that directory.

## Quality Requirements

- ALL commits must pass your project's quality checks (typecheck, lint, test)
- Do NOT commit broken code
- Keep changes focused and minimal
- Follow existing code patterns

## Browser Testing (If Available)

For any story that changes UI, verify it works in the browser if you have browser testing tools configured (e.g., via MCP):

1. Navigate to the relevant page
2. Verify the UI changes work as expected
3. Take a screenshot if helpful for the progress log

If no browser tools are available, note in your progress report that manual browser verification is needed.

## Stop Condition

After completing a user story, check if ALL stories have `passes: true`.

If ALL stories are complete and passing, reply with:
<promise>COMPLETE</promise>

If there are still stories with `passes: false`, end your response normally (another iteration will pick up the next story).

## Important

- Work on ONE story per iteration
- Commit frequently
- Keep CI green
- Read the Codebase Patterns section in progress.txt before starting


This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6 (6000.3.6f1) game project based on the Tanks Tutorial template. All game code lives in the `Tanks.Complete` namespace under `Assets/_Tanks/Scripts/`.

## Key Dependencies

- **Render Pipeline**: Universal Render Pipeline (URP) 17.3.0
- **Input**: New Input System 1.18.0 (not legacy Input Manager)
- **AI Navigation**: com.unity.ai.navigation 2.0.9 (NavMesh pathfinding)

## Architecture

**GameManager** (`Managers/GameManager.cs`) is the central orchestrator:
- Controls game state transitions (MainMenu ↔ Game)
- Manages round flow and win conditions
- Spawns tanks via TankManager array
- Updates camera targets

**TankManager** (`Managers/TankManager.cs`) wraps individual tank instances:
- Enables/disables tank controls between rounds
- Handles color assignment and reset
- Links input system to tank components

**Tank Components** (each tank has these MonoBehaviours):
- `TankMovement` - Rigidbody physics, input handling, particle effects
- `TankShooting` - Shell spawning with charge mechanic (min→max force)
- `TankHealth` - Damage, shields, death/explosion handling
- `TankAI` - NavMesh pathfinding with Seek/Flee states
- `TankInputUser` - Binds Input System actions to tank

## Input System Architecture

Uses ControlIndex system for multiplayer:
- ControlIndex 1: Left keyboard (WASD)
- ControlIndex 2: Right keyboard (Arrows)
- ControlIndex -1: Gamepad

Input actions defined in Input Action Asset, bound via `TankInputUser.cs`.

## Combat System

- Shells use `ShellExplosion.cs` with sphere overlap for damage detection
- Damage falloff: 100 at explosion center → 0 at explosion radius edge
- Explosion force applies physics push to affected tanks
- "Tank" layer used for filtering explosion targets

## Power-Up System

Six types defined in `PowerUpType` enum (`PowerUp.cs`):
1. Speed (+5 movement)
2. DamageReduction (50% shield)
3. ShootingBonus (-50% cooldown)
4. Healing (+20 HP)
5. Invincibility (temporary immunity)
6. DamageMultiplier (2x damage)

Power-ups spawn via `PowerUpSpawner`, display via `PowerUpHUD`, apply temporary buffs.

## Scenes

- `Assets/_Tanks/Scenes/Main.unity` - Primary gameplay scene
- `Assets/_Tanks/Tutorial_Demo/Demo_Scenes/` - Desert, Jungle, Moon demo scenes

## Prefab Locations

- Tanks: `Assets/_Tanks/Prefabs/Tanks/`
- Projectiles: `Assets/_Tanks/Prefabs/Explosives/`
- Power-ups: `Assets/_Tanks/Prefabs/PowerUps/`
- UI: `Assets/_Tanks/Prefabs/UI/`
- Environment: `Assets/_Tanks/Prefabs/Environment/`

## Code Patterns

- Private serialized fields use `m_` prefix (e.g., `m_Speed`)
- Public accessors expose serialized fields
- Tank colors set via "TankColor" named materials
- Camera uses orthographic with dynamic zoom to fit all tanks

## Common Gotchas

- Tank controls must be explicitly enabled via `TankManager.EnableControl()`
- Power-ups disabled during menu/pause phases via GameManager state
- Camera requires explicit URP overlay stack management in `GameUIHandler`
- AI pathfinding requires NavMesh baked in scene

## Ralph Automation

Autonomous development agent configured at `scripts/ralph/`:
- `prd.json` - Product requirements with user stories
- `progress.txt` - Execution history and codebase patterns
- `ralph.sh` - Automation orchestrator

Run with `/ralph` skill. Agent reads PRD, implements stories, commits changes.
