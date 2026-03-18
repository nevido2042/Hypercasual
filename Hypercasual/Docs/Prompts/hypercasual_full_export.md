# 하이퍼캐주얼(Hypercasual) 개발 문서 및 프롬프트 통합 내보내기 (Export)

본 문서는 하이퍼캐주얼 게임 개발 과정에서 사용된 모든 구현 계획서(Implementation Plans)와 작업 결과 보고(Walkthroughs)를 통합한 것입니다. 각 섹션은 기능별 또는 작업 단위별로 구분되어 있습니다.

---

## 목차
1. [핵심 시스템 (Core Systems)](#1-핵심-시스템-core-systems)
   - 젬스톤 스태킹 시스템 (Gemstone Stacking)
   - 바위 그리드 생성기 (Rock Grid Generator)
   - 오브젝트 풀링 (Object Pooling)
2. [AI 및 NPC 시스템 (AI & NPC Systems)](#2-ai-및-npc-시스템-ai--npc-systems)
   - 동적 죄수 생성 및 모델 변환 (Dynamic Prisoner)
   - AI 광부 채광 고도화 (Miner AI)
   - AI 크루 배송 로직 수정 (Crew AI Delivery)
3. [게임 로직 및 진행 (Game Logic & Progression)](#3-게임-로직-및-진행-game-logic--progression)
   - 순차적 잠금 해제 시스템 (Sequential Unlock)
   - 죄수 보상 상향 및 오디오 피드백 (Rewards & Audio)
4. [UI/UX 및 연출 (UI/UX & Visuals)](#4-uiux-및-연출-uiux--visuals)
   - 현금 가치 표시 UI (Cash UI)
   - 조작 유도(AFK) UI 및 애니메이션 (Movement Hint)
   - 애니메이션 깜빡임 현상 수정 (Animation Fixes)

---

## 1. 핵심 시스템 (Core Systems)

### 젬스톤 스태킹 시스템 (Gemstone Stacking)
**[구현 계획]**
- 캐릭터가 채광된 젬스톤을 수집하여 등 뒤에 수직으로 쌓는 시스템.
- `DOTween`을 사용하여 부드러운 이동 애니메이션과 급정거 시 "휘청(wobble)" 효과 구현.
- `PlayerStack.cs`: 스택 리스트 관리 및 위치 계산.
- `Gemstone.cs`: 비주얼 점프 및 물리 비활성화 처리.

### 바위 그리드 생성기 (Rock Grid Generator)
**[구현 계획]**
- 에디터 유틸리티를 통해 8x30 그리드로 바위 프리팹을 자동 배치.
- `RockGridGenerator.cs`: `[ContextMenu]`를 사용하여 인스펙터에서 버튼 클릭으로 생성.
- 간격(Spacing) 설정 가능.

### 오브젝트 풀링 (Object Pooling)
**[구현 계획]**
- 빈번하게 생성/파괴되는 젬스톤, 현금(Cash), 수갑(Handcuffs)에 적용.
- `EffectManager.cs` 및 `ReturnToPool` 컴포넌트 개선.
- `Instantiate/Destroy` 호출을 `Spawn/Release`로 대체하여 메모리 효율 개선.

---

## 2. AI 및 NPC 시스템 (AI & NPC Systems)

### 동적 죄수 생성 및 모델 변환 (Dynamic Prisoner)
**[구현 계획]**
- 고정된 프리팹 대신 무작위 비주얼 프리팹 생성 후 `Prisoner` 컴포넌트를 동적으로 부착.
- 수갑 수령 완료 시 공통 죄수복 모델(Uniform)로 즉시 교체.
- `PrisonerSpawner.cs`: 무작위 모델 선택 및 `AddComponent` 로직 처리.

### AI 광부 채광 고도화 (Miner AI)
**[작업 결과]**
- **데미지 기반 시스템**: `MineableRock`에 HP 추가. 플레이어는 1회, AI는 2회 타격 시 파괴.
- **애니메이션 동기화**: 애니메이션 타격 시점에 맞춰 사운드 및 `DOShake` 연출 실행.
- **전방 체크**: 캐릭터 전방 60도 범위 내 타겟만 조준하도록 정교화.

### AI 크루 배송 로직 수정 (Crew AI Delivery)
**[구현 계획 & 결과]**
- **동시성 이슈 해결**: `HandcuffsDeliveryZone`에서 다중 AI가 진입했을 때 `currentProvider`가 꼬이거나 컬렉션 수정 예외가 발생하는 문제 수정.
- `HashSet<Collider>` 및 안전한 복사본 순회 방식으로 변경.
- **깜빡임 방지**: 구역 내에서 불필요하게 `isStopped`를 토글하지 않도록 로직 최적화.

---

## 3. 게임 로직 및 진행 (Game Logic & Progression)

### 순차적 잠금 해제 시스템 (Sequential Unlock)
**[작업 결과]**
- **진행 단계 설정**:
  1. 튜토리얼 돈 수집 후 `MiningUpgradeZone` 활성화.
  2. 채광 업그레이드 1회 후 `MinerHireZone` 활성화.
  3. 광부 고용 후 `CrewHireZone` 활성화.
  4. 감옥 수용량(20명) 초과 시 `JailUpgradeZone` 활성화 및 카메라 연출.

### 죄수 보상 상향 및 오디오 피드백 (Rewards & Audio)
**[구현 계획 & 결과]**
- **보상 2배**: `rewardMultiplier`를 추가하여 요구 수갑 수의 2배로 현금 보상 지급.
- **오디오 정제**: 젬스톤 획득 시 소리는 제거하고, 배송/결제 시에만 tactile 피드백 제공.
- **가시성 기반 제한**: 화면 밖 AI의 사운드 출력을 제한하여 오디오 클러터(Clutter) 감소.

---

## 4. UI/UX 및 연출 (UI/UX & Visuals)

### 현금 가치 표시 UI (Cash UI)
**[구현 계획]**
- 플레이어가 보유한 현금 아이템 당 5의 가치를 계산하여 상단 패널에 표시.
- `PlayerStack.OnMoneyStackChanged` 이벤트를 구독하여 실시간 업데이트.
- `CashUI.cs`: 보유 갯수 * 5를 `TextMeshProUGUI`에 출력.

### 조작 유도(AFK) UI 및 애니메이션 (Movement Hint)
**[작업 결과]**
- 3초간 조작이 없을 경우 나타나는 드래그 이동 힌트.
- 손가락 아이콘이 **무한대(∞)** 기호를 그리며 움직이는 애니메이션 적용.
- 조작 감지 시 즉시 페이드 아웃 및 타이머 초기화.

### 애니메이션 깜빡임 현상 수정 (Animation Fixes)
**[작업 결과]**
- **정지 판정 통일**: `StoppingDistance`를 0.25m로 일관되게 적용하여 이동-정지 전환 시의 미세한 상태 변화 제거.
- **속도 임계값 상향**: 0.2f 이하의 미세 움직임은 `Run` 파라미터에 반영하지 않도록 수정.
- **회전 안정화**: `Slerp`를 사용하여 목표 방향으로 부드럽게 회전하도록 처리.

---

**참고 (Unity Guidelines):**
모든 코드는 하이퍼캐주얼 프로젝트 가이드라인을 준수하여 작성되었습니다.
- `[SerializeField] private` 사용 및 인스펙터 노출.
- C# 프로퍼티 기반 데이터 접근 및 `Hero` 네임스페이스 사용.
- 오브젝트 풀링 및 `DOTween.SetLink`를 이용한 생명 주기 관리.
