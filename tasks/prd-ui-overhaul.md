# PRD: Game UI Overhaul to Match Target Design

## Introduction

Overhaul the Tower Survivors in-game UI to match the target design screenshot (`scripts/ralph/Target.png`). This is a visual/layout-only redesign — no changes to game logic, economy balancing, or gameplay mechanics. The goal is to reposition, restyle, and add UI elements so the in-game HUD, shop, and weapon inventory match the target layout approximately.

### Target Screenshot Reference
`scripts/ralph/Target.png`

### Target Layout Summary
- **Top-left:** Wave counter ("WAVE 5")
- **Top-center:** Health bar with heart icon, red/yellow gradient bar, HP text ("720 / 1000")
- **Top-right:** Pause button
- **Bottom-left:** Upgrade icons row (3 icons with costs) + Level indicator ("Lv. 5" green button)
- **Bottom-center:** Compact shop grid (2 rows × 6 columns of item cards with icon + price)
- **Bottom-right:** Currency display with coin icon, Refresh button, Ability button (snowflake)
- **Very bottom:** "EQUIPPED" label with horizontal weapon icon slots

## Goals

- Reposition and restyle all HUD elements to match the target screenshot layout
- Replace the current shop list layout with a compact 2×6 grid
- Move the weapon inventory from a vertical left-side panel to a horizontal bottom bar
- Add new UI elements: level indicator, shop refresh button, ability button, upgrade quick-buy row
- Achieve an approximate visual match to the target (same general positioning and element grouping)
- Maintain all existing functional connections to game managers (no game logic changes)

## User Stories

### US-040: Redesign Top HUD Bar
**Description:** As a player, I want the top HUD to show the wave counter on the left, health bar in the center, and a pause button on the right, so the most critical info is clearly visible.

**Acceptance Criteria:**
- [ ] Wave counter displays in top-left as "WAVE [number]" with bold number styling
- [ ] Health bar is centered at top with a heart icon on its left side
- [ ] Health bar uses a red-to-yellow gradient fill (red = low HP, yellow = remaining capacity)
- [ ] HP text displays to the right of the bar as "[current] / [max]" (e.g., "720 / 1000")
- [ ] Pause button (|| icon) displays in top-right corner
- [ ] Remove or hide survival time text and wave progress slider from the top area (they are not in the target design)
- [ ] Remove or hide the "Next Wave" timer and "Enemies Remaining" text from the top area
- [ ] All elements update correctly from existing manager events (WaveManager, TowerHealth)
- [ ] Modify existing `HUD.cs` to implement these layout changes
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-041: Redesign Shop to Compact Grid Layout
**Description:** As a player, I want the shop displayed as a compact grid of item cards at the bottom-center of the screen, so I can quickly browse and buy items without a large overlay.

**Acceptance Criteria:**
- [ ] Shop displays as a 2-row × 6-column grid anchored to bottom-center of the screen
- [ ] Each shop item card shows: a small icon and a price number below it
- [ ] Item cards have dark semi-transparent backgrounds matching the target style
- [ ] Cards for weapons and upgrades are mixed together in the grid (no separate tabs)
- [ ] Items that cannot be afforded appear visually dimmed/grayed out
- [ ] Clicking/tapping a card purchases the item (same purchase logic as current ShopUI)
- [ ] The grid replaces the current tabbed scrollable list shop layout
- [ ] Modify existing `ShopUI.cs` and `ShopItemUI.cs` to implement the compact grid
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-042: Add Currency Display to Bottom-Right
**Description:** As a player, I want to see my current gold/currency in the bottom-right corner with a coin icon, so I always know what I can afford.

**Acceptance Criteria:**
- [ ] Currency display shows in bottom-right area with a coin/currency icon
- [ ] Amount is formatted with commas (e.g., "1,250")
- [ ] Display updates in real-time when gold changes (subscribes to GoldManager.OnGoldChanged)
- [ ] Move gold display from its current top-left position to bottom-right
- [ ] Modify existing `HUD.cs` to reposition the gold counter
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-043: Add Shop Refresh Button
**Description:** As a player, I want a refresh button near the shop area so I can re-roll the available shop items.

**Acceptance Criteria:**
- [ ] Circular refresh/re-roll button (circular arrows icon) displays in bottom-right, below the currency display
- [ ] Button is clickable and triggers a shop refresh (re-randomizes available items in the grid)
- [ ] Add a `RefreshShop()` method to `ShopUI.cs` that randomizes which items appear in the 12 grid slots
- [ ] Button has a visual pressed/hover state
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-044: Add Ability Button
**Description:** As a player, I want a special ability button in the bottom-right UI area so I can activate abilities during gameplay.

**Acceptance Criteria:**
- [ ] Ability button (snowflake/star icon) displays in bottom-right, below the refresh button
- [ ] Button is a non-functional placeholder for now (logs a message or shows a "Coming Soon" tooltip on click)
- [ ] Button has a visual pressed/hover state
- [ ] Button icon matches the target screenshot style (blue/cyan tint)
- [ ] Create new script `AbilityButtonUI.cs` in the UI scripts folder if needed, or add to `HUD.cs`
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-045: Add Upgrade Quick-Buy Row
**Description:** As a player, I want a row of upgrade icons in the bottom-left that I can quickly purchase without opening the full shop.

**Acceptance Criteria:**
- [ ] 3 upgrade icons display in a horizontal row at the bottom-left of the screen
- [ ] Each icon shows the upgrade's icon and its cost below it (e.g., "200", "150", "300")
- [ ] Icons are sourced from existing UpgradeData assets
- [ ] Clicking an icon purchases the upgrade (same logic as current upgrade purchasing via UpgradeManager)
- [ ] Icons that cannot be afforded appear dimmed
- [ ] Icons update after purchase (cost increases for stackable upgrades)
- [ ] Create new script `UpgradeQuickBuyUI.cs` or add to existing `ShopUI.cs`
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-046: Add Level Indicator
**Description:** As a player, I want to see my current level displayed in the bottom-left corner below the upgrade row, so I know my progression.

**Acceptance Criteria:**
- [ ] Level indicator displays below the upgrade quick-buy row in bottom-left
- [ ] Shows an upward arrow icon and "Lv. [number]" text
- [ ] Green background styling matching the target screenshot
- [ ] Level value is read from an existing source (e.g., wave number, or a dedicated level value if one exists)
- [ ] If no level system exists, display the current wave number as the level (UI-only, no new game logic)
- [ ] Modify existing `HUD.cs` or create minimal `LevelIndicatorUI.cs`
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-047: Redesign Weapon Inventory to Horizontal Bottom Bar
**Description:** As a player, I want my equipped weapons displayed as a horizontal row of icons at the very bottom of the screen with an "EQUIPPED" label, so I can see my loadout at a glance.

**Acceptance Criteria:**
- [ ] Weapon inventory displays as a horizontal bar at the very bottom of the screen
- [ ] "EQUIPPED" label appears on the left side of the bar
- [ ] Each equipped weapon shows as a small colored icon (color matches weapon rarity)
- [ ] Up to 8 weapon slots visible in a horizontal row
- [ ] Empty slots appear as dim/empty placeholders
- [ ] Weapon fire pulse animation still works (existing feature)
- [ ] Replaces the current vertical left-side weapon inventory layout
- [ ] Modify existing `WeaponInventoryUI.cs` to implement horizontal layout
- [ ] Use unityMCP tool to verify layout in Unity Editor

### US-048: Apply Dark Theme and Color Scheme
**Description:** As a player, I want the UI to use the dark theme with red/cyan accent colors shown in the target, so the game has a cohesive visual style.

**Acceptance Criteria:**
- [ ] Overall UI background panels use dark semi-transparent backgrounds (#1a1a2e or similar dark tone)
- [ ] Health bar uses red fill with yellow/orange for depleted portion
- [ ] Currency icon and text use a warm gold/orange color
- [ ] Ability button uses cyan/blue tint
- [ ] Level indicator uses green background
- [ ] Shop item cards use dark card backgrounds with subtle borders
- [ ] Text uses light/white color for readability against dark backgrounds
- [ ] Update colors across all modified UI scripts and prefabs
- [ ] Use unityMCP tool to verify visual appearance in Unity Editor

## Functional Requirements

- FR-1: Reposition wave counter to top-left, styled as "WAVE [number]" with bold number
- FR-2: Reposition and restyle health bar to top-center with heart icon and gradient fill
- FR-3: Add pause button to top-right corner
- FR-4: Replace shop list layout with 2×6 compact grid at bottom-center
- FR-5: Each shop grid card displays item icon and price only
- FR-6: Move currency display to bottom-right with coin icon and comma-formatted number
- FR-7: Add shop refresh/re-roll button in bottom-right below currency
- FR-8: Add placeholder ability button in bottom-right below refresh button
- FR-9: Add upgrade quick-buy row (3 icons with costs) in bottom-left
- FR-10: Add level indicator ("Lv. X") with green styling below upgrade row in bottom-left
- FR-11: Reposition weapon inventory to horizontal bottom bar with "EQUIPPED" label
- FR-12: Apply dark theme color scheme across all UI elements
- FR-13: All UI elements must remain functional — connected to existing game managers and events
- FR-14: Shop item affordability visual feedback must still work (dimmed when unaffordable)

## Non-Goals (Out of Scope)

- No changes to game logic, economy, or balancing
- No changes to enemy behavior, weapon mechanics, or wave system
- No new gameplay features (ability button is placeholder only)
- No changes to MainMenuUI, GameOverUI, or PauseMenuUI screens (only the in-game HUD)
- No changes to mobile controls or input handling
- No new manager scripts or game systems
- No changes to ScriptableObject data assets (WeaponData, UpgradeData, etc.)
- No audio or sound effect changes

## Design Considerations

- **Reference screenshot:** `scripts/ralph/Target.png`
- **Layout precision:** Approximate match — same general positioning, grouping, and proportions
- **Color palette:** Dark backgrounds (#1a1a2e), red health bar, gold/orange currency, cyan ability, green level indicator, white text
- **Existing components to reuse:** TextMeshPro for all text, Unity UI Image for icons and backgrounds, existing prefab structure where possible
- **Icon assets:** If specific icons (heart, coin, pause, refresh, snowflake, arrow) are not available in the project, use TextMeshPro text symbols or simple geometric shapes as placeholders

## Technical Considerations

- Modify `HUD.cs` for top bar redesign, gold repositioning, and level indicator
- Modify `ShopUI.cs` and `ShopItemUI.cs` for compact grid layout and refresh functionality
- Modify `WeaponInventoryUI.cs` for horizontal bottom bar layout
- Create new `UpgradeQuickBuyUI.cs` for the upgrade quick-buy row (if not feasible to add to existing scripts)
- Create new `AbilityButtonUI.cs` for the placeholder ability button (if not feasible to add to HUD.cs)
- All UI changes should be done via Unity's RectTransform anchoring system for responsive positioning
- Existing event subscriptions (OnGoldChanged, OnGameStateChanged, OnHealthChanged, etc.) must be preserved
- Use the unityMCP tool for all Unity Editor operations (creating/modifying GameObjects, adjusting RectTransforms, setting up UI components)

## Success Metrics

- In-game HUD layout approximately matches the target screenshot
- All existing UI functionality is preserved (health display, gold updates, shop purchasing, weapon inventory)
- No errors or missing references when running the game
- UI elements are correctly anchored and do not overlap or clip at standard resolutions

## Open Questions

- Are specific icon sprite assets available in the project for heart, coin, pause, refresh, snowflake, and arrow icons? If not, text/shape placeholders will be used.
- Should the shop refresh button have a gold cost, or is it free? (Defaulting to free for now since this is UI-only)
- What determines which 3 upgrades appear in the quick-buy row? (Defaulting to first 3 available upgrades)
