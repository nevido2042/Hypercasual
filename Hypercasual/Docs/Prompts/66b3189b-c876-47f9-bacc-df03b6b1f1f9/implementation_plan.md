# Rock Grid Generator Implementation

The goal is to provide a way to arrange `Rock` prefabs in an 8x30 grid. This will be implemented as an editor-utility script that can be used directly from the Unity Editor.

## Proposed Changes

### Gameplay Environment

#### [NEW] [RockGridGenerator.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Utils/RockGridGenerator.cs)
- Create a new script with an `ContextMenuItem` or `Button` (via `[ContextMenu]`) to generate rocks.
- Grid size: 8 (columns/X) x 30 (rows/Z).
- Spacing: Configurable, defaulting to 1.5 - 2.0 units.
- Logic: Instantiate the `Rock.prefab` and set its position in a nested loop.

## Verification Plan

### Manual Verification
- Add the script to an empty GameObject in the scene.
- Assign the `Rock.prefab` to the serialized field.
- Click the "Generate Grid" context menu in the Inspector.
- Verify 240 rocks are spawned and correctly aligned.
