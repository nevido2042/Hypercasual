# 프로젝트 안정화 및 Cash UI 구현 완료

죄수(Prisoner) 및 크루(Crew)의 애니메이션 깜빡임 현상을 수정하고, 플레이어가 보유한 현금 가치를 실시간으로 UI에 표시하는 기능을 구현했습니다.

## 주요 변경 사항

### [Prisoner.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/Prisoner.cs)
- `MoveTo` 메서드 및 `FixedUpdate`의 도달 거리 판정 기준(`StoppingDistance`)을 **0.25m**로 통일하여, 정지 상태에서 이동 명령을 받았을 때 발생하는 애니메이션 깜빡임 현상을 완전히 해결했습니다.
- 루트 객체에 있던 `Animator` 컴포넌트를 제거하고, 시각적 모델의 애니메이터를 사용하도록 변경하여 본(Bone) 제어 문제를 해결했습니다.

### [CrewAI.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/CrewAI.cs)
- `Update` 메서드에서 애니메이션 재생 조건에 `remainingDistance` 체크를 추가하고 속도 임계값을 높여(0.2f), 정지 시의 미세한 떨림으로 인한 애니메이션 깜빡임을 방지했습니다.
- 목표 지점에 이미 도달한 경우 불필요한 `SetDestination` 호출을 건너뛰도록 로직을 최적화했습니다.
- 회전 방식을 `Slerp`를 이용한 부드러운 회전으로 변경하여 시각적 안정성을 높였습니다.
- 초기화 시 전달받은 `RuntimeAnimatorController`와 `Avatar`를 내부 필드에 저장하여, 비주얼 모델이 교체될 때마다 즉시 적용되도록 구현했습니다.
- `SetupModelAnimator` 메서드를 신설하여 모델 생성 시 애니메이터 초기화와 현재 이동 상태(`Run` 파라미터) 동기화를 일괄 처리합니다.

### [PrisonerSpawner.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/PrisonerSpawner.cs)
- 불필요해진 `PrisonerVisualData` 구조체를 제거하고, `prisonerVisuals` 멤버 변수를 단순 `GameObject[]` 배열로 변경했습니다.
- 인스펙터에서 설정된 `uniformAvatar`를 공통 아바타로 사용하도록 로직을 단순화했습니다.
- 모든 비주얼 모델이 생성될 때 동일한 아바타 설정을 유지합니다.

---

### Cash UI 구현
- **계층 구조 정리:** 기존 "Gold Panel"의 명칭을 유저 요청에 따라 **"Cash Panel"**로 변경했습니다.
- **[PlayerStack.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/PlayerStack.cs):** 현금 갯수를 반환하는 `MoneyCount` 프로퍼티와 스택 변경 시 이벤트를 발생시키는 `OnMoneyStackChanged`를 추가했습니다.
- **[CashUI.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/CashUI.cs) (신규):** 보유 현금 수에 비례하여(갯수 * 5) UI 텍스트를 실시간으로 업데이트하는 컴포넌트를 작성했습니다.
- **컴포넌트 연결:** "Cash Panel"에 `CashUI`를 부착하고, `Player`와 `Text (TMP)`를 바인딩하여 기능을 완성했습니다.

## 검증 결과

- **코드 안정성:** 스크립트 컴파일 오류가 없음을 확인했습니다.
- **애니메이션 연출:** 죄수 생성 시 인스펙터에 할당된 공통 아바타와 컨트롤러가 적용되어 이동 및 대기 애니메이션이 정상 작동합니다.
- **비주얼 교체:** 수갑을 다 채운 후 제복 모델로 바뀔 때도 기존의 애니메이터 설정을 유지하므로 애니메이션이 끊기지 않습니다.

> [!TIP]
> 이제 인스펙터의 `PrisonerSpawner`에서 개별 비주얼마다 아바타를 넣을 필요 없이, `Uniform Avatar` 필드 하나만 관리하면 됩니다.
