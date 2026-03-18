# Task: 배달 구역 적재 제한 구현

## Research and Planning
- [x] 관련 스크립트 분석 (`GemstoneDeliveryZone`, `HandcuffsStackZone`, `MachineController`)
- [x] 구현 계획 수립 및 사용자 승인 완료

## Implementation
- [x] `GemstoneDeliveryZone.cs`: 최대 적재량 및 MAX 텍스트 로직 추가
- [x] `HandcuffsStackZone.cs`: 최대 적재량 및 IsFull 프로퍼티 추가
- [x] `MachineController.cs`: 출력 구역 만크 시 생산 중단 로직 추가
- [x] `FloatingText.cs`: `SetupPersistent` 및 `Hide` 메서드 추가
- [x] `GemstoneDeliveryZone.cs` & `HandcuffsStackZone.cs`: 지속적인 "MAX" 표시 로직 구현

## AI Miner 개선
- [x] Crew AI 배달 존 대기 현상(Soft-lock) 수정
    - [x] HandcuffsDeliveryZone 다중 deliverer 처리 리팩토링
    - [x] CrewAI 대기 로직 및 효율성 개선
- [x] AI 광부 채광 로직 및 사운드 개선
    - [x] `MineableRock.cs`: 내구도 시스템 구현
    - [x] `MinerAI.cs`: 애니메이션 이벤트 기반 채광 및 사운드 구현
    - [x] `PlayerMining.cs`: AI 변경사항에 따른 호환성 확인
- [x] AI 광부 채광 성능 검증 (2회 타격 확인)

## Verification
- [x] 젬스톤 배달 구역 가득 찼을 때 "MAX" 표시 확인
- [x] 수갑 적재 구역 가득 찼을 때 기계 생산 중단 확인
- [x] 적재 공간 확보 시 자동 재가동 확인
- [x] 컴파일 오류 및 경고 없음 확인
- [x] "MAX" 텍스트 높이 조절 (플레이어: 1.2, 구역: 오프셋 제거)
- [x] 구역 가득 찼을 때 "MAX" 텍스트가 사라지지 않고 유지되는지 확인
- [x] 공간 확보 시 "MAX" 텍스트가 즉시 사라지는지 확인
- [x] 조작 힌트 UI 구현
    - [x] 힌트 이미지 생성 (`drag_to_move_hint`)
    - [x] 힌트 UI 스크립트 작성 (`MovementHintUI.cs`)
    - [x] 무한대(∞) 모양 이동 애니메이션 구현
    - [x] UI 프리팹 구성 및 캔버스 배치 (에디터 내 수동 설정 안내 완료)
- [x] 튜토리얼 마커 이동시간 단축 (1.0s -> 0.5s)
- [x] 업그레이드 존 UI 표시 및 비용 차별화
    - [x] 텍스트 표시 갯수 5배 적용 (*5)
    - [x] 드릴(4개), 드릴카(10개) 레벨별 비용 설정
- [x] 시네마틱 및 특정 UI 오픈 시 AFK 힌트 억제 및 컴파일 오류 수정
