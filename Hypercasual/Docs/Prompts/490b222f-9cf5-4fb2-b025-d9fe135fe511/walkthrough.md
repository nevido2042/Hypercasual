# 고용 구역 및 자동 광부 구현 완료

현금을 지불하여 자동으로 채광을 수행하는 광부를 고용하는 시스템을 구현했습니다.

## 구현 내용
- **광부 애니메이션**: 상/하체 분리 애니메이션(Layered Animation)을 적용하여 이동 중에도 채광이 가능하며, 곡괭이를 항상 장착하도록 개선되었습니다.
- **젬스톤 배달 자동화**: 광부가 채광한 젬스톤은 플레이어에게 오지 않고 맵의 'Gemstone Delivery Zone'으로 직접 전달되어 쌓입니다.

### 1. 고용 구역 (Hire Zone)
- 플레이어가 구역에 진입하면 현금이 하나씩 소모됩니다 (총 5개).
- 지불 진행 상황이 UI(텍스트 및 게이지)로 표시됩니다.
- 지불 완료 시 지정된 스폰 지점에서 광부 3명이 생성됩니다.
- 고용 완료 후 구역은 비활성화됩니다.

### 2. 자동 채광 광부 (Miner AI)
- 생성된 광부들은 `NavMesh`를 사용하여 가장 가까운 바위를 자동으로 찾아 이동합니다.
- 바위에 근접하면 곡괭이가 나타나며 "Mining" 애니메이션을 재생합니다.
- 실제 채광 로직(`rock.Mine()`)을 주기적으로 실행하여 아이템을 생성합니다.
- 주변에 바위가 없으면 대기 상태로 전환됩니다.

## 주요 파일 및 설정
- [MinerAI.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/MinerAI.cs): 자동 채무 로직 및 툴 가시성 제어.
- [HireZone.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/HireZone.cs): 지불 프로세스 및 광부 스포너.
- [Miner.prefab](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/00.Prefabs/Miner.prefab): `MinerAI` 컴포넌트 추가 및 `Pickaxe` 오브젝트 부착 완료.
- [Hire Zone (Scene Object)](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/02.Scenes/Game.unity): 씬 내 배치 및 UI 요소 연결 완료.

## 추가 수정 및 버그 해결 (버전 1.1)

### 젬스톤 생성 버그 해결
- **문제**: 광부(Miner) 오브젝트가 `Player` 태그를 가지고 있어, 채광 로직이 잘못된 스택에 아이템을 넣으려 시도함.
- **해결**: 
    - `Miner.prefab`의 태그를 `Untagged`로 수정했습니다.
    - [MineableRock.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/MineableRock.cs)에서 태그 기반 탐색 대신 `Object.FindFirstObjectByType<PlayerStack>()`을 사용하여 실제 플레이어 스택을 직접 찾도록 로직을 강화했습니다.

### 광부 이동 문제 해결 (NavMesh)
- **문제**: 씬에 길 찾기 데이터(NavMesh)가 없어 광부들이 이동하지 못함.
- **해결**:
    - 바닥(`Plane`) 오브젝트를 Static으로 설정했습니다.
    - 에디터 메뉴 `Hero/Bake NavMesh`를 추가하고 실행하여 NavMesh 베이킹을 완료했습니다.

### 바위 리스폰 시간 조정
- **변경 사항**: 코드에 고정되어 있던 리스폰 시간을 인스펙터 필드로 분리했습니다.
- **사용 방법**: `Rock` 프리팹의 `MineableRock` 컴포넌트 내 **Respawn Time** 필드에서 원하는 시간을 초 단위로 설정할 수 있습니다. (기본값: 5.0)

### 테스트용 치트키 수정 (키 충돌 해결)
- 기존 'G' 키가 메인 카메라의 회전 기능과 겹치는 문제를 해결하기 위해 치트키를 변경했습니다.
- **J 키**: 플레이어에게 현금 10개를 즉시 지급합니다.
- **K 키**: 플레이어에게 젬스톤 10개를 즉시 지급합니다.

## 동작 확인 방법
1. 게임 실행 후 **'J' 키**를 눌러 현금이 쌓이는지 확인합니다.
2. **'K' 키**를 눌러 젬스톤이 쌓이는지 확인합니다.
3. 바위를 채굴한 후, 약 **5초** 뒤에 다시 리스폰되는지 확인합니다.
