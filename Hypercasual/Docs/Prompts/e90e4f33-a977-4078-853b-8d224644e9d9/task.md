# Task: Fix DOTween Safe Mode Errors during Rock Regeneration

- [x] Analyze `MineableRock.cs` for DOTween usage and regeneration logic <!-- id: 0 -->
- [x] Analyze `PlayerMining.cs` for mining detection logic <!-- id: 1 -->
- [x] Identify the cause of missing tween targets <!-- id: 2 -->
- [x] Implement fix (Kill tweens on disable/destroy or set link) <!-- id: 3 -->
- [x] Debug and fix DOLocalJump crash in delivery/consume zones <!-- id: 6 -->
- [x] Verify completeness of DOTween safety across scripts <!-- id: 5 -->
- [x] Updating walkthrough <!-- id: 4 -->

## Crew AI Implementation
- [x] Prepare `HandcuffsConsumeZone` and `HandcuffsStackZone` for AI interaction <!-- id: 7 -->
- [x] Implement `CrewAI` script with state machine logic <!-- id: 8 -->
- [x] Create `CrewHireZone` for spawning Crew <!-- id: 9 -->
- [x] Refactor `CrewAI` to remove `PrisonerQueueManager` and use Zone checks <!-- id: 13 -->
- [x] Ensure `HandcuffsDeliveryZone` works with Crew standing on it <!-- id: 14 -->
- [x] Refactor `CrewAI` to stock ConsumeZone and activate DeliveryZone <!-- id: 15 -->
- [x] Ensure prisoners only receive handcuffs when they have arrived at the queue start point <!-- id: 17 -->
- [x] Ensure Crew AI stops moving while picking up handcuffs from the stack zone <!-- id: 18 -->
- [x] Make Crew AI rotate instantly toward its movement direction <!-- id: 19 -->
- [x] Parent pooled objects to `ObjectPoolingManager` with per-object grouping <!-- id: 20 -->
- [x] Implement `JailController` and `JailSensor` for automated door management <!-- id: 21 -->
- [x] Implement `JailController` and `JailSensor` for automated door management <!-- id: 21 -->
- [x] Setup Jail sensors and UI in the scene <!-- id: 22 -->
- [x] Refactor `JailController` to manage leaving prisoners and control door dynamically <!-- id: 23 -->
- [x] Update `Prisoner` to register with `JailController` when satisfied <!-- id: 24 -->
- [x] Verify dynamic door behavior and clean up scene sensors <!-- id: 25 -->
- [x] Fix prisoner rotation issue by freezing Rigidbody Y-axis rotation <!-- id: 26 -->
- [x] Fix slow door animation issue using `isAnimating` guard <!-- id: 27 -->
- [x] Prevent duplicate prisoner counting using `HasEnteredJail` flag <!-- id: 29 -->
- [x] Refine prisoner animation (bool) and jail entry positioning <!-- id: 30 -->
- [x] Add random offsets to queue and set default orientation to back <!-- id: 31 -->
- [x] Implement timeout for final jail destination to prevent getting stuck <!-- id: 32 -->
- [x] Set initial camera rotation to -45 degrees (Alpha2 state) <!-- id: 33 -->
- [x] Refine orientation logic: only face back inside the jail <!-- id: 34 -->
- [x] Change jail count text format to "0/20" <!-- id: 35 -->
- [x] Implement prisoner visual variation and state change <!-- id: 36 -->
    - [x] Modify `Prisoner.cs` for dynamic initialization, model swapping, and explicit Inspector slots for Avatar/Controller <!-- id: 37 -->
    - [x] Modify `PrisonerSpawner.cs` for dynamic component attachment and `PrisonerVisualData` slots <!-- id: 38 -->
