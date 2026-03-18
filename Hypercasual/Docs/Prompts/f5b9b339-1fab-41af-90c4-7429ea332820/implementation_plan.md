# Gemstone Stacking System Implementation Plan

Character will now collect gemstones from mined rocks. These gemstones will stack on the player's back using DOTween for smooth animations and a "wobble" effect when the player stops moving.

## User Review Required
> [!IMPORTANT]
> The wobble effect will be triggered based on the player's movement deceleration. I will use `EightDirectionMovement`'s input magnitude to detect stops.

## Proposed Changes

### [Gemstone & Stacking Components]

#### [NEW] [Gemstone.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gemstone.cs)
- Handles the visual "jump" from the rock to the player.
- Disables physics/colliders once collected.

#### [NEW] [PlayerStack.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/PlayerStack.cs)
- Manages the list of collected gemstones.
- Calculates stack positions (stacking vertically).
- Implements the "휘청(wobble)" effect using DOTween when the player stops.
- References `EightDirectionMovement` to detect movement state.

#### [MODIFY] [MineableRock.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/MineableRock.cs)
- Add `gemstonePrefab` field.
- Instantiate gemstone and initiate collection when mined.

### [Player Configuration]
- Add `PlayerStack` component to the `Player` GameObject.
- Set up a `StackPoint` transform on the player's back.

## Verification Plan

### Manual Verification
1.  **Collection**: Mine a rock and verify the gemstone flies to the player.
2.  **Stacking**: Mine multiple rocks and verify they stack vertically on the back.
3.  **Wobble Effect**: Move the player and then stop suddenly. Verify the stack sways or wobbles.
