# 동적 죄수 생성 및 모델 변환 구현 계획 (컴포넌트 추가 방식)

미리 만들어진 죄수 프리팹을 사용하는 대신, 무작위 비주얼 프리팹을 먼저 생성한 뒤 `Prisoner` 컴포넌트를 동적으로 부착하여 죄수를 생성합니다. 또한 수갑 수령 완료 시 공통 죄수복 모델로 변환합니다.

## 제안된 변경 사항

### [Gameplay]

#### [MODIFY] [Prisoner.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/Prisoner.cs)
- `[SerializeField]` 필드들을 동적으로 설정할 수 있도록 `Initialize` 메서드를 추가합니다.
    - `minRequired`, `maxRequired`, `moveSpeed`, `uniformModelPrefab` 등을 인자로 받습니다.
- `visualContainer`가 인스펙터에서 할당되지 않은 경우, 실행 시점에 자식 오브젝트로 자동 생성하도록 수정합니다.
- 동적으로 부착된 비주얼 프리팹의 `Animator`를 `Awake` 또는 `Initialize` 시점에 자동으로 찾도록 개선합니다.
- 기존의 `SetVisuals` 로직을 동적 생성 흐름에 맞게 조정합니다.

#### [MODIFY] [PrisonerSpawner.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/PrisonerSpawner.cs)
- `basePrisonerPrefab` 참조를 제거합니다. (이제 비주얼 프리팹이 기반이 됩니다.)
- `SpawnPrisoner()` 로직을 다음과 같이 수정합니다:
    1. `characterModelPrefabs` 리스트에서 무작위 비주얼 프리팹을 선택하여 생성합니다.
    2. 생성된 오브젝트에 `Prisoner` 컴포넌트를 `AddComponent`로 부착합니다.
    3. 부착된 `Prisoner` 컴포넌트의 `Initialize`를 호출하여 필요한 설정값과 `uniformModelPrefab`을 전달합니다.

## 검증 계획

### 수동 검증
- 죄수가 생성될 때 무작위 비주얼 프리팹으로 생성되고 정상적으로 이동/대기하는지 확인합니다.
- 생성된 오브젝트에 `Prisoner`, `Rigidbody`, `CapsuleCollider` 컴포넌트가 올바르게 부착되었는지 확인합니다.
- 수갑을 모두 채웠을 때 죄수복 모델로 정상적으로 교체되는지 확인합니다.
- 모델 교체 후에도 애니메이션이 끊김 없이 작동하는지 확인합니다.
