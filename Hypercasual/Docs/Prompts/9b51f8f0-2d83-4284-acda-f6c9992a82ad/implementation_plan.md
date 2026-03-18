# Crew AI Delivery Soft-lock Fix

## Problem
Crew AI agents get stuck in the `HandcuffsDeliveryZone`. This is primarily due to:
1.  **Multiple Provider Logic**: `HandcuffsDeliveryZone` only tracks and pulls from a single `currentProvider`. If multiple crew members enter, only the first one is served. Others stay forever waiting for their `hcStack` to clear.
2.  **Stale Provider Reference**: If the first provider finishes but remains in the zone, the zone won't start serving the second provider.
3.  **Strict Wait Condition**: Crew AI waits until the `HandcuffsConsumeZone` is idle, which can delay them getting more handcuffs even if they are empty.

## Proposed Changes

### [Gameplay]

#### [MODIFY] [HandcuffsDeliveryZone.cs](file:///d:/Fork_Git/Hypercasual\Hypercasual\Assets\01.Scripts\Gameplay\HandcuffsDeliveryZone.cs)
- **동시 수정 문제 해결**: `ConsumeRoutine`에서 `_activeColliders`를 순회하는 도중 `OnTriggerExit`에 의해 컬렉션이 변경되어 발생하는 `InvalidOperationException`을 수정합니다.
- 순회 전 `_activeColliders`의 복사본을 만들어 안전하게 순회하도록 변경합니다.

#### [MODIFY] [CrewAI.cs](file:///d:/Fork_Git/Hypercasual\Hypercasual\Assets\01.Scripts\Gameplay\CrewAI.cs)
- **깜빡임 문제 해결**: `BehaviorSequence`에서의 우선순위와 `ActionMoveToDeliveryZone`의 종료 조건이 충돌하여 0.2초마다 상태가 바뀌는 현상을 수정합니다.
- `BehaviorSequence` 우선순위 조정: `비어있을 때 충전(ActionTakeFromStack)`을 `단순 구역 활성화(ActionMoveToDeliveryZone)`보다 높게 설정합니다.
- `ActionMoveToDeliveryZone` 내에서 이미 구역에 있을 경우 불필요하게 `isStopped`를 풀었다가 다시 멈추는 로직을 제거하여 애니메이션 튐 현상을 방지합니다.
- 행동 루프 간의 `waitSmall` 시간을 최적화하거나, 상태 전환 시 즉각 반응하도록 개선합니다.

## Verification Plan

### Manual Verification
- Deploy 2+ Crew AI agents.
- Observe them both entering the delivery zone.
- Verify both are correctly served and return to the stack zone.
- Verify they don't get stuck in a "waiting" state when they could be fetching more items.
