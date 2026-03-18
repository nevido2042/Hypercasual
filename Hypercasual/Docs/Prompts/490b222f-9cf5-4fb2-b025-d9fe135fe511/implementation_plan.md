# 아이템 오브젝트 풀링 적용 계획

빈번하게 생성 및 파괴되는 젬스톤, 현금(Cash), 수갑(Handcuffs) 아이템에 오브젝트 풀링을 적용하여 메모리 효율과 성능을 개선합니다.

## 제안된 변경 사항

### [Core] EffectManager & ReturnToPool
- `EffectManager.cs` 내의 `ReturnToPool` 컴포넌트 개선:
    - 반환 시 `DOKill()`을 호출하여 잔여 트윈 제거
    - 로컬 스케일 등을 초기화하여 재사용 시 시각적 오류 방지

### [Gameplay] 생성 지점 (Instantiate -> Spawn)
- `MineableRock.cs`: 젬스톤 생성 시 `Spawn` 사용
- `MachineController.cs`: 수갑 생성 시 `Spawn` 사용
- `MoneyStackZone.cs`: 현금 생성 시 `Spawn` 사용
- `CheatManager.cs`: 현금 및 젬스톤 생성 시 `Spawn` 사용

### [Gameplay] 소멸 지점 (Destroy -> Release)
- `MachineController.cs`: 소모된 젬스톤 반환
- `PlayerStack.cs`: 스택 초과 시 젬스톤 반환
- `Prisoner.cs`: 소모된 수갑 반환
- `HireZone.cs`: 지불된 현금 반환
- `MiningUpgradeZone.cs`: 지불된 현금 반환

## 검증 계획
- 현금, 젬스톤, 수갑이 정상적으로 생성되고 쌓이는지 확인
- 아이템이 소모된 후 씬에서 사라지고 다시 생성될 때 위치/크기가 정상적인지 확인
- "MAX" 상태나 업그레이드 시 아이템이 풀로 정상 반환되는지 확인
