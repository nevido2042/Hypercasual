# Upgrade System & Audio Refinement Walkthrough

## Completed Changes

### 1. 순차적 잠금 해제 시스템 (Sequential Unlock)
- **초기 상태 (Default Inactive)**: 모든 업그레이드 존(채광, 광부, 크루, 감옥 확장)은 시작 시 **비활성화(Inactive)** 상태로 설정됩니다.
- **채광 업그레이드 등장**: 튜토리얼 진행 중 돈 수집 단계가 끝나면 `MiningUpgradeZone`이 처음으로 나타납니다.
- **광부 고용 등장**: 채광 업그레이드를 **1회**만 수행해도 `MinerHireZone`이 즉시 활성화됩니다.
- **크루 고용 등장**: 광부 고용 결제가 완료되면 `CrewHireZone`이 활성화됩니다.
- **감옥 확장 등장**: 감옥 수용량(20명)이 가득 차면 `JailUpgradeZone`이 활성화되며 전용 카메라 연출이 실행됩니다.

### 2. Audio Feedback Improvements
- **Mining Sound**: "Mining.wav" now triggers precisely during the pickaxe hit animation. Miner AI sounds are optimized to play only when visible on screen.
- **Gemstone Stacking**: Removed the sound when gemstones are added to the player's back. Sound now plays **only when gemstones are delivered/dropped**.
- **Handcuffs & Money**: Sound plays for both adding and removing/paying, maintaining clear tactile feedback.
- **Prisoner Interaction**: Added "Stack.wav" feedback when a prisoner is successfully handcuffed at the **HandcuffsConsumeZone**.

### 3. Visual & Logic Fixes
- **Jail Capacity**: The jail now correctly stops admitting prisoners and spawning them when the count hits 20. The UI count turns red as a warning.
- **Tutorial Markers**: Fixed scale distortion issues by decoupling markers from their parent transforms and using world-space tracking.

## Verified Features
- [x] Mining upgrade level 1 unlocks miner hire.
- [x] All sequence triggers work without breaking existing camera animations.
- [x] Audio feels less cluttered by removing redundant gemstone pickup sounds.
- [x] Prison capacity logic is robust and prevents overflow.
