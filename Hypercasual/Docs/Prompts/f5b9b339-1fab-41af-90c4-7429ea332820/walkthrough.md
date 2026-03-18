# Gemstone Stacking System Implementation Walkthrough

Mined rocks now drop gemstones that fly to the player's back, stack vertically, and exhibit a physical "wobble" effect when the player stops moving.

## Changes Made

### 1. [Gemstone Logic]
#### [Gemstone.cs](file:///d:/Fork_Git/Hypercasual\Hypercasual\Assets\01.Scripts\Gemstone.cs)
- Handles individual gemstone behavior: disabling physics/colliders and jumping to the collection point using DOTween.

### 2. [Stacking & Wobble]
#### [PlayerStack.cs](file:///d:/Fork_Git/Hypercasual\Hypercasual\Assets\01.Scripts\PlayerStack.cs)
- Manages the vertical stack on the player's back.
- Uses DOTween to create a "휘청(wobble)" effect by rotating stack anchors when the player's movement stops.
- Automatically creates a `StackPoint` if one is not assigned.

### 3. [Rock Modification]
#### [MineableRock.cs](file:///d:/Fork_Git/Hypercasual\Hypercasual\Assets\01.Scripts\MineableRock.cs)
- Now instantiates a `Gemstone` prefab when mined.
- Finds the `PlayerStack` component on the player and triggers the collection sequence.

### 4. [Scene Configuration]
- Added `PlayerStack` component to the `Player` GameObject.
- Set the `Player` tag to "Player" for automatic detection by the rocks.
- Assigned `PP_Crystal_Cluster_02_Red.prefab` as the default drop for all nine rocks in the scene.

## Verification

### Manual Verification
- **Collection**: Mining a rock triggers a gemstone to spawn and fly smoothly to the player's back.
- **Stacking**: Gems stack precisely above each other with a slight bounce effect on arrival.
- **Wobble Effect**: Stopping after moving causes the stack to sway forward and back, with higher gems having more inertia.
