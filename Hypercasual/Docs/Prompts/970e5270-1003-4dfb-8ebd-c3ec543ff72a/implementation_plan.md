# 죄수 보상 2배 상향 계획

죄수가 만족하여 퇴장할 때 지급하는 현금 보상의 양을 기존의 2배로 상향합니다.

## 제안된 변경 사항

### Gameplay

#### [MODIFY] [Prisoner.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/Prisoner.cs)
- `rewardMultiplier` 시리얼라이즈 필드 추가 (기본값 2).
- `OnSatisfied` 메서드에서 `moneyZone.SpawnCash` 호출 시 `requiredHandcuffs * rewardMultiplier`를 인자로 전달하도록 수정.

## 검증 계획

### 수동 검증
- 게임 내에서 죄수에게 수갑을 전달하고, 퇴장 시 생성되는 현금 뭉치의 개수가 요구한 수갑 수의 2배(예: 수갑 3개 요구 시 현금 6개 생성)가 되는지 확인합니다.
