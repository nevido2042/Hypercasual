### [Audio Feedback System]

#### [MODIFY] [MachineController.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/MachineController.cs)
- `AudioClip productionSound` 필드 추가.
- `ProcessGem` 메서드 내에서 수갑 생산(스폰) 시 효과음 출력.

#### [MODIFY] [BasePaymentZone.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/BasePaymentZone.cs)
- `AudioClip completeSound` 필드 추가.
- `OnPaymentFinished` 이벤트 호출 전 혹은 `OnPaymentComplete` 시점에 효과음 출력.
- 이를 통해 `MiningUpgradeZone`, `JailUpgradeZone`, `MinerHireZone`, `CrewHireZone` 모두 개별 효과음 설정 가능.

### [Bug Fix & Optimization]

#### [MODIFY] [HandcuffsDeliveryZone.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/HandcuffsDeliveryZone.cs)
- `HashSet<Collider> _activeColliders`를 사용하여 다중 콜라이더 추적.
- `exitGraceTime` (기본 0.5초) 추가: 모든 콜라이더가 나갔을 때 즉시 종료하지 않고 대기.
- 대기 중 다른 콜라이더가 다시 들어오면 종료 프로세스 취소 및 코루틴 유지.
- 이를 통해 경계에서의 미세한 움직임(지터링)으로 인한 반복 재시작 방지.

#### [MODIFY] [CrewAI.cs](file:///d:/Fork_Git\Hypercasual\Hypercasual\Assets\01.Scripts\Gameplay\CrewAI.cs)
- `ActionMoveToDeliveryZone` 내에서 구역 진입 시 `agent.velocity = Vector3.zero`를 명시적으로 호출.
- `ActionTakeFromStack`에서도 수갑 스택존 진입 시 즉시 정지(`agent.velocity = Vector3.zero`)하도록 수정.
- `OnTriggerEnter/Exit`에 `HandcuffsStackZone` 판별 로직 추가 및 `isInStackZone` 변수 활용.
