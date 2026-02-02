# PRD: Tower Survivors

## Introduction

Tower Survivors is a roguelike tower defense game inspired by Vampire Survivors and Halls of Torment. The player controls a stationary tower at the center of the screen while waves of enemies rush in from all directions. Unlike traditional survivors games, the player cannot move - instead, they must strategically purchase weapons, upgrades, and power-ups from a permanent shop using gold that accumulates over time. Weapons fire automatically at enemies within range. The goal is to survive as long as possible, with difficulty escalating every 30 seconds through stronger enemy waves and increased gold generation.

This game will be built using the existing Unity Tanks tutorial project assets, repurposing tanks as enemies, shells as projectiles, and existing visual effects for a cohesive experience.

## Goals

- Create an engaging endless survival experience with roguelike elements
- Implement a satisfying gold economy that rewards survival time
- Provide meaningful strategic choices through weapon variety and upgrades
- Achieve smooth 60fps gameplay with dozens of enemies on screen
- Reuse existing Tanks project assets to minimize art requirements
- Track high scores to encourage replayability

## User Stories

> **IMPORTANT FOR RALPH**: All user stories involving Unity assets (scenes, prefabs, materials, ScriptableObjects, UI) MUST use the Unity MCP tools. The pattern is:
> 1. Write C# scripts using file editing tools
> 2. Use Unity MCP to create/configure Unity assets and wire up components
> 3. Never edit `.unity`, `.prefab`, `.asset`, or `.meta` files as text

### Phase 1: Core Game Loop

#### US-001: Create Tower Survivors Scene
**Description:** As a developer, I need a new scene set up for Tower Survivors so that I have a clean foundation separate from the original Tanks game.

**Acceptance Criteria:**
- [ ] Use Unity MCP `create_scene` to create new scene at `Assets/_Tanks/Scenes/TowerSurvivors.unity`
- [ ] Use Unity MCP to add a flat ground plane (reuse desert or jungle ground assets)
- [ ] Use Unity MCP to add proper lighting (directional light)
- [ ] Use Unity MCP to position camera for top-down view with slight tilt (45-60 degree angle)
- [ ] Camera is static, centered on play area
- [ ] Use Unity MCP `bake_navmesh` for enemy pathfinding
- [ ] Scene loads without errors (verify via Unity MCP)

---

#### US-002: Implement Player Tower
**Description:** As a player, I want a stationary tower in the center of the screen so that I have something to defend.

**Acceptance Criteria:**
- [ ] Write `TowerHealth.cs` script in `Assets/_Tanks/Scripts/TowerSurvivors/Tower/`
- [ ] Use Unity MCP to duplicate Tank prefab, remove movement components
- [ ] Use Unity MCP to create Tower prefab at `Assets/_Tanks/Prefabs/TowerSurvivors/Tower.prefab`
- [ ] Use Unity MCP to add TowerHealth component to Tower prefab
- [ ] Use Unity MCP to place Tower at center of play area (0, 0, 0)
- [ ] Tower has health system (starting HP: 100)
- [ ] Tower cannot move (no input handling for movement)
- [ ] Use Unity MCP to add HealthSlider UI above tower
- [ ] Tower plays death animation/explosion when HP reaches 0 (reuse TankExplosion)

---

#### US-003: Implement Basic Enemy
**Description:** As a player, I want enemies that move toward my tower so that I have something to defend against.

**Acceptance Criteria:**
- [ ] Write `Enemy.cs` script in `Assets/_Tanks/Scripts/TowerSurvivors/Enemies/`
- [ ] Write `EnemyData.cs` ScriptableObject in `Assets/_Tanks/Scripts/TowerSurvivors/Enemies/`
- [ ] Use Unity MCP to duplicate Tank Enemy Variant, modify color (orange)
- [ ] Use Unity MCP to create Enemy prefab at `Assets/_Tanks/Prefabs/TowerSurvivors/Enemies/BasicEnemy.prefab`
- [ ] Use Unity MCP to add NavMeshAgent and Enemy components
- [ ] Use Unity MCP to create BasicEnemyData ScriptableObject asset
- [ ] Enemy uses NavMesh pathfinding to move toward tower
- [ ] Enemy has health (starting HP: 20)
- [ ] Enemy deals damage on collision with tower (10 damage)
- [ ] Enemy dies when HP reaches 0 (plays explosion effect via Unity MCP prefab reference)
- [ ] Enemy drops gold on death (base: 5 gold)

---

#### US-004: Implement Enemy Spawner
**Description:** As a developer, I need enemies to spawn around the play area so that waves of enemies attack the tower.

**Acceptance Criteria:**
- [ ] Enemies spawn at random positions around the edge of the play area
- [ ] Spawn positions form a circle/rectangle around the tower (radius ~30 units)
- [ ] Spawner respects wave timing (new wave every 30 seconds)
- [ ] Base enemy count per wave: 5, increases by 3 each wave
- [ ] Enemies spawn with slight delays (0.5s between each) for visual effect
- [ ] `EnemySpawner.cs` script created in `Assets/_Tanks/Scripts/Enemies/`

---

#### US-005: Implement Gold System
**Description:** As a player, I want to earn gold passively and from killing enemies so that I can purchase weapons and upgrades.

**Acceptance Criteria:**
- [ ] Player starts with 50 gold
- [ ] Gold earned passively: 1 gold per second
- [ ] Gold generation increases every 30 seconds (+1 gold/sec per wave)
- [ ] Gold earned from enemy kills (varies by enemy type)
- [ ] Gold total displayed in UI (top-left corner)
- [ ] `GoldManager.cs` script created in `Assets/_Tanks/Scripts/Economy/`

---

#### US-006: Implement Game State Manager
**Description:** As a developer, I need a central game manager for Tower Survivors to control game flow and state.

**Acceptance Criteria:**
- [ ] `TowerSurvivorsGameManager.cs` created in `Assets/_Tanks/Scripts/Managers/`
- [ ] Manages game states: MainMenu, Playing, Paused, GameOver
- [ ] Tracks current wave number
- [ ] Tracks elapsed time (survival time)
- [ ] Triggers wave transitions every 30 seconds
- [ ] Handles game over when tower dies
- [ ] Displays "Game Over" screen with survival time and waves survived

---

#### US-007: Implement Basic Weapon System
**Description:** As a player, I want weapons that automatically fire at enemies so that I can defend my tower.

**Acceptance Criteria:**
- [ ] `Weapon.cs` base class created in `Assets/_Tanks/Scripts/Weapons/`
- [ ] Weapons have: damage, fire rate, range, projectile type
- [ ] Weapons automatically target nearest enemy in range
- [ ] Weapons fire projectiles at target (reuse Shell prefab)
- [ ] Tower can have multiple weapons equipped
- [ ] `WeaponManager.cs` tracks all equipped weapons

---

#### US-008: Create First Weapon - Cannon
**Description:** As a player, I want a basic cannon weapon so that I can start defending my tower.

**Acceptance Criteria:**
- [ ] Cannon weapon prefab created
- [ ] Stats: 15 damage, 1.5s fire rate, 15 unit range
- [ ] Uses existing shell/projectile visuals
- [ ] Projectile has area damage on impact (small radius)
- [ ] Visual feedback: muzzle flash, projectile trail
- [ ] Tower starts with one Cannon equipped by default

---

### Phase 2: Shop & Economy

#### US-009: Implement Shop UI
**Description:** As a player, I want a shop panel so that I can purchase weapons and upgrades with my gold.

**Acceptance Criteria:**
- [ ] Shop panel visible on right side of screen during gameplay
- [ ] Shop shows available items with: name, cost, description
- [ ] Items grayed out if player cannot afford them
- [ ] Click/tap to purchase (deducts gold, adds item)
- [ ] Shop scrollable if more items than fit on screen
- [ ] `ShopUI.cs` script created in `Assets/_Tanks/Scripts/UI/`

---

#### US-010: Implement Weapon Shop Items
**Description:** As a player, I want to buy different weapons from the shop so that I can increase my firepower.

**Acceptance Criteria:**
- [ ] At least 5 different weapons available in shop
- [ ] Weapons have different damage types (Normal, Piercing, Magic)
- [ ] Weapons have different attack patterns (single target, AoE, rapid fire)
- [ ] Weapons have rarity tiers affecting stats (Common, Uncommon, Rare)
- [ ] Price scales with rarity (Common: 50-100, Uncommon: 150-250, Rare: 300-500)
- [ ] Purchased weapons automatically equip and start firing

---

#### US-011: Implement Upgrade Shop Items
**Description:** As a player, I want to buy upgrades that improve my tower's stats so that I can survive longer.

**Acceptance Criteria:**
- [ ] At least 5 different upgrades available in shop
- [ ] Upgrade types: Max HP, HP Regen, Gold Generation, Damage Boost, Attack Speed
- [ ] Upgrades can be purchased multiple times (stacking)
- [ ] Each upgrade has diminishing returns or caps
- [ ] Upgrade effects immediately apply when purchased
- [ ] `UpgradeManager.cs` script created in `Assets/_Tanks/Scripts/Economy/`

---

### Phase 3: Enemy Variety

#### US-012: Implement Fast Enemy Type
**Description:** As a player, I want fast enemies that are harder to hit so that combat is more varied.

**Acceptance Criteria:**
- [ ] Fast enemy prefab created (smaller, different color - e.g., light blue)
- [ ] Movement speed: 2x base enemy speed
- [ ] Health: 50% of base enemy (10 HP)
- [ ] Damage: 50% of base enemy (5 damage)
- [ ] Gold drop: 3 gold
- [ ] Begins spawning from wave 3

---

#### US-013: Implement Tank Enemy Type
**Description:** As a player, I want tanky enemies that are hard to kill so that I need to manage my damage output.

**Acceptance Criteria:**
- [ ] Tank enemy prefab created (larger, different color - e.g., dark green)
- [ ] Movement speed: 50% of base enemy speed
- [ ] Health: 3x base enemy (60 HP)
- [ ] Damage: 2x base enemy (20 damage)
- [ ] Gold drop: 15 gold
- [ ] Begins spawning from wave 5

---

#### US-014: Implement Ranged Enemy Type
**Description:** As a player, I want ranged enemies that attack from distance so that positioning matters.

**Acceptance Criteria:**
- [ ] Ranged enemy prefab created (medium size, different color - e.g., purple)
- [ ] Stops at distance from tower (10 units) and shoots
- [ ] Fires projectiles at tower (reuse shell, smaller scale)
- [ ] Health: same as base enemy (20 HP)
- [ ] Projectile damage: 8 damage
- [ ] Gold drop: 8 gold
- [ ] Begins spawning from wave 7

---

#### US-015: Implement Boss Enemy
**Description:** As a player, I want boss enemies at milestone waves so that there are exciting challenges.

**Acceptance Criteria:**
- [ ] Boss enemy prefab created (2x scale of base tank, red color)
- [ ] Health: 500 HP (scales +100 per appearance)
- [ ] Damage: 50 damage on collision
- [ ] Movement speed: 75% of base enemy
- [ ] Spawns alone at waves 10, 20, 30, etc.
- [ ] Gold drop: 100 gold (scales +50 per appearance)
- [ ] Visual indicator when boss spawns (screen shake, warning text)

---

### Phase 4: Weapon Variety

#### US-016: Implement Machine Gun Weapon
**Description:** As a player, I want a rapid-fire weapon for dealing with many weak enemies.

**Acceptance Criteria:**
- [ ] Machine Gun weapon prefab created
- [ ] Stats: 5 damage, 0.2s fire rate, 12 unit range
- [ ] Single target, rapid fire
- [ ] Rarity: Common, Cost: 75 gold
- [ ] Uses smaller projectile visual

---

#### US-017: Implement Laser Weapon
**Description:** As a player, I want a piercing weapon that can hit multiple enemies.

**Acceptance Criteria:**
- [ ] Laser weapon prefab created
- [ ] Stats: 20 damage, 2s fire rate, 20 unit range
- [ ] Piercing damage type - hits all enemies in a line
- [ ] Rarity: Uncommon, Cost: 200 gold
- [ ] Visual: instant beam effect (LineRenderer)

---

#### US-018: Implement Mortar Weapon
**Description:** As a player, I want an area damage weapon for groups of enemies.

**Acceptance Criteria:**
- [ ] Mortar weapon prefab created
- [ ] Stats: 30 damage, 3s fire rate, 25 unit range
- [ ] Large area of effect (5 unit radius)
- [ ] Projectile arcs through the air
- [ ] Rarity: Rare, Cost: 350 gold
- [ ] Uses larger explosion effect

---

#### US-019: Implement Tesla Coil Weapon
**Description:** As a player, I want a chain lightning weapon for unique gameplay.

**Acceptance Criteria:**
- [ ] Tesla Coil weapon prefab created
- [ ] Stats: 12 damage, 1.5s fire rate, 10 unit range
- [ ] Magic damage type - chains to 3 nearby enemies
- [ ] Each chain deals 75% of previous damage
- [ ] Rarity: Rare, Cost: 400 gold
- [ ] Visual: lightning effect between targets

---

### Phase 5: Polish & UI

#### US-020: Implement Main Menu
**Description:** As a player, I want a main menu so that I can start the game and see my high scores.

**Acceptance Criteria:**
- [ ] Main menu scene or overlay
- [ ] "Start Game" button begins gameplay
- [ ] High score display (best survival time, best wave reached)
- [ ] Game title prominently displayed
- [ ] Clean, readable UI using existing Tanks UI style

---

#### US-021: Implement HUD
**Description:** As a player, I want a clear HUD showing game state so that I know how I'm doing.

**Acceptance Criteria:**
- [ ] Gold counter (top-left)
- [ ] Wave number (top-center)
- [ ] Survival time (top-center, below wave)
- [ ] Tower health bar (bottom-center or near tower)
- [ ] Equipped weapons display (left side, icons)
- [ ] All UI elements readable and non-obtrusive

---

#### US-022: Implement Pause Menu
**Description:** As a player, I want to pause the game so that I can take breaks.

**Acceptance Criteria:**
- [ ] ESC key or pause button pauses game
- [ ] Time.timeScale set to 0 when paused
- [ ] Pause overlay with "Resume" and "Quit to Menu" buttons
- [ ] Shop still visible but purchases disabled while paused

---

#### US-023: Implement Game Over Screen
**Description:** As a player, I want a game over screen so that I can see my final stats and try again.

**Acceptance Criteria:**
- [ ] Game over overlay appears when tower dies
- [ ] Shows: survival time, waves survived, enemies killed, gold earned
- [ ] "Play Again" button restarts game
- [ ] "Main Menu" button returns to menu
- [ ] Updates high score if new record

---

#### US-024: Implement Weapon Inventory Display
**Description:** As a player, I want to see my equipped weapons so that I know my current loadout.

**Acceptance Criteria:**
- [ ] Weapon icons displayed on left side of screen
- [ ] Shows weapon name on hover/hold
- [ ] Visual indication of weapon firing (icon pulses or highlights)
- [ ] Max 8 weapons displayed (scrollable if more)

---

### Phase 6: Persistence & Balance

#### US-025: Implement High Score Persistence
**Description:** As a player, I want my high scores saved so that I can track my progress over time.

**Acceptance Criteria:**
- [ ] High scores saved to PlayerPrefs
- [ ] Tracks: best survival time, best wave reached, total enemies killed (lifetime)
- [ ] Scores persist between game sessions
- [ ] New record notification when high score beaten

---

#### US-026: Implement Wave Scaling
**Description:** As a developer, I need proper difficulty scaling so that the game remains challenging.

**Acceptance Criteria:**
- [ ] Enemy health increases 10% per wave
- [ ] Enemy damage increases 5% per wave
- [ ] Enemy speed increases 2% per wave (capped at +50%)
- [ ] Enemy count per wave scales logarithmically
- [ ] Mix of enemy types changes based on wave number
- [ ] Scaling values configurable in ScriptableObject

---

#### US-027: Implement Weapon Balancing System
**Description:** As a developer, I need weapons defined as data so that they can be easily balanced.

**Acceptance Criteria:**
- [ ] `WeaponData` ScriptableObject created
- [ ] All weapon stats defined in ScriptableObjects
- [ ] Weapons reference their data, not hardcoded values
- [ ] Easy to adjust stats without code changes
- [ ] At least 8 weapons fully defined with balanced stats

---

## Functional Requirements

- FR-1: The tower must remain stationary at the center of the play area at all times
- FR-2: Enemies must pathfind toward the tower using Unity NavMesh
- FR-3: Weapons must automatically acquire and fire at valid targets within range
- FR-4: Gold must accumulate passively at a rate that increases every 30 seconds
- FR-5: The shop must be accessible at all times during gameplay (not paused)
- FR-6: Purchases must immediately deduct gold and apply effects
- FR-7: Game over must trigger when tower health reaches zero
- FR-8: Wave transitions must occur every 30 seconds with visual/audio feedback
- FR-9: All enemy and weapon stats must be data-driven (ScriptableObjects)
- FR-10: High scores must persist between game sessions using PlayerPrefs

## Non-Goals (Out of Scope)

- Player movement or direct control of weapons
- Multiplayer or co-op modes
- Multiple maps or biomes (single arena only for MVP)
- Save/load mid-game (only high scores persist)
- Achievement system
- In-app purchases or monetization
- Full 90 weapons (8-10 for this scope)
- Weapon upgrade/evolution system (future feature)
- Sound effects and music (can be added later)
- Mobile touch controls (desktop/keyboard focus)
- Tutorial or onboarding flow

## Technical Considerations

### Unity MCP Integration (REQUIRED)

**Ralph MUST use the Unity MCP (Model Context Protocol) server for all Unity Editor operations.** Do not attempt to manually edit scene files, prefabs, or other Unity assets as text/YAML - this will corrupt them.

The Unity MCP provides tools for:
- **Scene Management**: Creating scenes, adding/removing GameObjects, setting up hierarchy
- **Prefab Operations**: Creating prefabs, instantiating prefabs, modifying prefab instances
- **Component Management**: Adding components to GameObjects, setting component properties
- **Asset Operations**: Creating materials, ScriptableObjects, and other assets
- **NavMesh**: Baking NavMesh for enemy pathfinding
- **Camera Setup**: Positioning and configuring cameras
- **UI Canvas**: Creating UI elements, Canvas setup, layout management

**Workflow for each story:**
1. Write C# scripts using standard file editing tools
2. Use Unity MCP to create/modify scenes, prefabs, and wire up components
3. Use Unity MCP to assign script references, configure serialized fields
4. Use Unity MCP to test play mode if needed

**Common Unity MCP operations for this project:**
- `create_scene` - Create the TowerSurvivors scene
- `create_gameobject` - Create tower, enemies, UI elements
- `add_component` - Attach scripts to GameObjects
- `set_component_property` - Configure component values
- `create_prefab` - Save configured GameObjects as prefabs
- `instantiate_prefab` - Spawn prefabs in scene
- `create_scriptable_object` - Create WeaponData, EnemyData assets
- `bake_navmesh` - Generate NavMesh for enemy pathfinding

**CRITICAL**: Never edit `.unity`, `.prefab`, `.asset`, or `.meta` files directly. Always use Unity MCP tools.

---

### Asset Reuse Strategy
- **Tower**: Modified Tank prefab with movement disabled
- **Enemies**: Tank Enemy Variant with color/scale modifications
- **Projectiles**: CompleteShell prefab with modified properties
- **Explosions**: TankExplosion prefab for deaths, ShellExplosion for impacts
- **UI**: Existing HealthSlider, adapt Menus prefab patterns
- **Ground**: Desert or Jungle ground prefabs

### Architecture
- New namespace: `TowerSurvivors` to separate from `Tanks.Complete`
- Managers: `TowerSurvivorsGameManager`, `GoldManager`, `WaveManager`, `WeaponManager`, `UpgradeManager`
- Data: ScriptableObjects for `WeaponData`, `EnemyData`, `UpgradeData`, `WaveConfig`
- Use object pooling for projectiles and enemies to maintain performance

### Performance Targets
- Support 50+ enemies on screen at 60fps
- Object pooling for all frequently spawned objects
- Efficient targeting system (spatial partitioning if needed)

### File Structure

**Scripts** (create via file editing):
```
Assets/_Tanks/Scripts/TowerSurvivors/
├── Core/
│   └── TowerSurvivorsGameManager.cs
├── Tower/
│   ├── Tower.cs
│   └── TowerHealth.cs
├── Enemies/
│   ├── Enemy.cs
│   ├── EnemySpawner.cs
│   └── EnemyData.cs (ScriptableObject class definition)
├── Weapons/
│   ├── Weapon.cs
│   ├── WeaponManager.cs
│   ├── WeaponData.cs (ScriptableObject class definition)
│   └── Projectile.cs
├── Economy/
│   ├── GoldManager.cs
│   ├── UpgradeManager.cs
│   └── UpgradeData.cs (ScriptableObject class definition)
├── Waves/
│   ├── WaveManager.cs
│   └── WaveConfig.cs (ScriptableObject class definition)
└── UI/
    ├── ShopUI.cs
    ├── HUD.cs
    └── GameOverUI.cs
```

**Unity Assets** (create via Unity MCP):
```
Assets/_Tanks/Prefabs/TowerSurvivors/
├── Tower.prefab
├── Enemies/
│   ├── BasicEnemy.prefab
│   ├── FastEnemy.prefab
│   ├── TankEnemy.prefab
│   ├── RangedEnemy.prefab
│   └── BossEnemy.prefab
├── Weapons/
│   ├── Cannon.prefab
│   ├── MachineGun.prefab
│   ├── Laser.prefab
│   ├── Mortar.prefab
│   └── TeslaCoil.prefab
└── UI/
    └── (UI prefabs as needed)

Assets/_Tanks/Data/TowerSurvivors/
├── Enemies/
│   ├── BasicEnemyData.asset
│   ├── FastEnemyData.asset
│   └── ...
├── Weapons/
│   ├── CannonData.asset
│   ├── MachineGunData.asset
│   └── ...
└── Waves/
    └── WaveConfig.asset

Assets/_Tanks/Scenes/
└── TowerSurvivors.unity
```

## Success Metrics

- Player can survive at least 5 waves on first playthrough
- Average session length of 5-10 minutes
- Core gameplay loop (spawn → kill → earn → buy → survive) feels satisfying
- No performance drops below 60fps with 50 enemies on screen
- Shop purchases feel impactful and strategic
- Players want to "try one more time" after game over

## Open Questions

1. Should weapons have limited ammo or be unlimited once purchased?
2. Should there be a "sell weapon" feature to recoup some gold?
3. What should the maximum number of equipped weapons be? (Suggested: 8)
4. Should certain weapon combinations create synergies?
5. Should there be a "prestige" or meta-progression system for future updates?
6. Should enemies have damage types that are strong/weak against certain weapons?
