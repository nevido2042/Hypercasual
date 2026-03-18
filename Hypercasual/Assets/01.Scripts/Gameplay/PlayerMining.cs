using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 플레이어의 채광(Mining) 관련 로직 전담 클래스
    /// </summary>
    public class PlayerMining : MonoBehaviour
    {
        [Header("채광 스탯")]
        [SerializeField] private float miningRange = 1.5f;     // 바위 탐색 및 타격 사거리
        [SerializeField] private int maxMineTargets = 1;       // 한 번에 타격 가능한 최대 목표 수 (업그레이드용)
        [SerializeField] private LayerMask rockLayer;          // 바위 오브젝트들이 속한 물리 레이어

        [Header("채광 도구")]
        [SerializeField] private GameObject miningTool;        // 손에 들 곡괭이 오브젝트
        [SerializeField] private GameObject drillPrefab;       // 업그레이드용 드릴 프리팹
        [SerializeField] private GameObject drillCarPrefab;    // 업그레이드용 드릴카 프리팹
        [SerializeField] private float carBoardHeightOffset = 0.45f; // 드릴카 탑승 시 전체 높이 오프셋
        [SerializeField] private Vector3 carSeatLocalPos = new Vector3(0, 1.1f, -0.6f); // 드릴카 내부 좌석 위치
        [SerializeField] private Vector3 carTrunkStackOffset = new Vector3(0, 0.5f, -1.2f); // 드릴카 트렁크 스택 오프셋

        [Header("오디오 설정")]
        [SerializeField] private AudioClip miningSound;        // 채광 사운드 (Mining.wav)
        
        private AudioSource audioSource;
        private PlayerStack _playerStack;
        private UnityEngine.AI.NavMeshAgent _agent;
        private float _defaultAgentOffset;

        private bool isMining = false; // 현재 주변에 캘 바위가 있는지 상태
        private int upgradeTier = 0;   // 0: 곡괭이, 1: 드릴, 2: 드릴카
        private DrillHead drillInstance;      // 생성된 드릴 인스턴스 참조
        private GameObject drillCarInstance;
        private RockGridGenerator miningGrid; // 추가: 채광 구역 참조
        
        public bool IsMining => isMining;
        public bool IsDrillUpgraded => upgradeTier >= 1; // 1단계 이상(드릴/드릴카) 여부
        public int UpgradeTier => upgradeTier;           // 현재 업그레이드 티어
        public bool IsBoardingDrillCar => upgradeTier == 2 && isMining; // 드릴카 실제 탑승 여부 extension.

        void Awake()
        {
            // 설정된 곡괭이가 없다면 자식 중에서 'Pickaxe' 이름을 탐색
            if (miningTool == null)
            {
                Transform foundTool = transform.FindDeepChild("Pickaxe");
                if (foundTool != null) miningTool = foundTool.gameObject;
            }

            // 시작 시 곡괭이 비활성화
            if (miningTool != null) miningTool.SetActive(false);

            // 오디오 소스 설정
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            _playerStack = GetComponent<PlayerStack>();
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (_agent != null) _defaultAgentOffset = _agent.baseOffset;
        }

        void Start()
        {
            miningGrid = Object.FindFirstObjectByType<RockGridGenerator>();
        }

        void Update()
        {
            CheckForRocks(); // 구역 진입 확인 (애니메이션 전환 및 도구 시각화)
        }

        /// <summary>
        /// 플레이어가 RockGridGenerator구역 내부에 있는지 확인
        /// </summary>
        void CheckForRocks()
        {
            if (miningGrid == null)
            {
                isMining = false;
                return;
            }

            // 영역 내부에 있는지 먼저 체크
            bool insideGrid = miningGrid.IsInsideGrid(transform.position);

            if (!insideGrid)
            {
                isMining = false;
            }
            else
            {
                // 곡괭이 모드(upgradeTier 0)일 때는 전방에 실제 캘 수 있는 바위가 있을 때만 채광 상태로 진입
                if (upgradeTier == 0)
                {
                    isMining = HasTargetRockInFront();
                }
                else
                {
                    // 드릴/드릴카 모드일 때는 구역 내부에만 있으면 채광 상태 유지
                    isMining = true;
                }
            }

            // 가시성 업데이트
            if (upgradeTier == 1)
            {
                // 드릴 모드: 드릴 활성화 및 회전 제어
                if (drillInstance != null)
                {
                    if (isMining != drillInstance.gameObject.activeSelf)
                    {
                        drillInstance.gameObject.SetActive(isMining);
                    }
                    drillInstance.SetActiveMining(isMining);
                }
                
                // 곡괭이는 드릴 업그레이드 시 항상 꺼둠
                if (miningTool != null && miningTool.activeSelf) miningTool.SetActive(false);
            }
            else if (upgradeTier == 2)
            {
                // 드릴카 모드: 주변에 캘 바위가 있을 때만 차량이 나타나고 탑승함
                if (drillCarInstance != null)
                {
                    if (isMining != drillCarInstance.activeSelf)
                    {
                        drillCarInstance.SetActive(isMining);
                        UpdateCharacterPositionForCar(isMining);
                        
                        // 추가: 드릴카 내부의 DrillHead 상태도 명시적으로 업데이트
                        DrillHead carDrill = drillCarInstance.GetComponentInChildren<DrillHead>();
                        if (carDrill != null) carDrill.SetActiveMining(isMining);
                    }
                }
                
                // 곡괭이는 항상 꺼둠
                if (miningTool != null && miningTool.activeSelf) miningTool.SetActive(false);
            }
            else
            {
                // 곡괭이 모드
                if (miningTool != null)
                {
                    if (isMining != miningTool.activeSelf)
                    {
                        miningTool.SetActive(isMining);
                    }
                }
            }
        }

        private void UpdateCharacterPositionForCar(bool boarding)
        {
            Transform characterRoot = transform.Find("Bip001");
            if (characterRoot == null) characterRoot = transform.FindDeepChild("Bip001");
            
            if (characterRoot != null)
            {
                if (boarding)
                {
                    // [개선] 부드러운 탑승 연출 (DOTween)
                    characterRoot.DOKill();
                    characterRoot.DOLocalMove(carSeatLocalPos, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
                    
                    // NavMeshAgent 오프셋을 높여 전체 모델(차량 포함)을 지면 위로 들어올림
                    if (_agent != null) _agent.baseOffset = _defaultAgentOffset + carBoardHeightOffset;

                    // 스택 위치를 드릴카 트렁크 위치로 조정
                    if (_playerStack != null)
                    {
                        _playerStack.SetStackPointsOffset(carTrunkStackOffset);
                    }
                }
                else
                {
                    // [개선] 부드러운 하차 연출
                    characterRoot.DOKill();
                    characterRoot.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutQuad).SetLink(gameObject);
                    
                    // NavMeshAgent 오프셋 원복
                    if (_agent != null) _agent.baseOffset = _defaultAgentOffset;

                    // 스택 위치 원복
                    if (_playerStack != null)
                    {
                        _playerStack.SetStackPointsOffset(Vector3.zero);
                    }
                }
                characterRoot.DOKill(false); // 회전은 즉시 리셋
                characterRoot.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 전방 사거리 내에 실제 캘 수 있는 바위가 있는지 확인
        /// </summary>
        private bool HasTargetRockInFront()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
            foreach (var hitCollider in hitColliders)
            {
                MineableRock rock = hitCollider.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    Vector3 dirToRock = (rock.transform.position - transform.position).normalized;
                    dirToRock.y = 0;
                    float dot = Vector3.Dot(transform.forward, dirToRock);
                    
                    // 전방 약 60도 범위 내에 바위가 있으면 true
                    if (dot > 0.5f) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 애니메이터 타격 이벤트 시점에 호출되어 실제 바위를 채굴하는 함수
        /// </summary>
        public void PerformMiningHit()
        {
            // 사운드 재생
            if (audioSource != null && miningSound != null)
            {
                audioSource.PlayOneShot(miningSound);
            }

            // 드릴/드릴카 업그레이드 완료 후에는 DrillHead가 직접 채굴하므로 무시함
            if (upgradeTier >= 1) return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
            List<MineableRock> rocksInRange = new List<MineableRock>();

            // 범위 내 캘 수 있는 모든 바위 수집 및 전방 체크
            foreach (var hitCollider in hitColliders)
            {
                MineableRock rock = hitCollider.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    // 전방 방향 체크 (Dot Product)
                    Vector3 dirToRock = (rock.transform.position - transform.position).normalized;
                    dirToRock.y = 0;
                    float dot = Vector3.Dot(transform.forward, dirToRock);

                    // 약 60도(cos 60 = 0.5) 정도의 전방 범위만 허용
                    if (dot > 0.5f)
                    {
                        rocksInRange.Add(rock);
                    }
                }
            }

            // 플레이어와 가장 가까운 순서대로 정렬
            rocksInRange.Sort((a, b) => 
                Vector3.Distance(transform.position, a.transform.position).CompareTo(
                Vector3.Distance(transform.position, b.transform.position)));

            // maxMineTargets 수만큼 순차적으로 채광 처리
            int minedCount = 0;
            foreach (var rock in rocksInRange)
            {
                if (minedCount >= maxMineTargets) break;
                
                rock.Mine(2, gameObject); // 바위 채굴 실행 (플레이어는 데미지 2)
                minedCount++;
            }
        }

        /// <summary>
        /// 채광 능력 업그레이드 (티어별 확장)
        /// </summary>
        public void UpgradeMining()
        {
            upgradeTier++;
            
            if (upgradeTier == 1)
            {
                CreateDrill();
            }
            else if (upgradeTier == 2)
            {
                CreateDrillCar();
            }

            // 적재량 증가 추가
            if (_playerStack != null)
            {
                _playerStack.IncreaseCapacity(10);
            }

            maxMineTargets += 2; // 티어 상승 시 타격 대상 크게 증가
            miningRange += 0.5f; // 사거리 증가
            
            // 시각적 피드백
            if (upgradeTier == 1 && drillInstance != null)
            {
                drillInstance.transform.DOKill();
                drillInstance.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f).SetLink(drillInstance.gameObject);
            }
            else if (upgradeTier == 2 && drillCarInstance != null)
            {
                drillCarInstance.transform.DOKill();
                drillCarInstance.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f).SetLink(drillCarInstance.gameObject);
            }
            else if (miningTool != null)
            {
                miningTool.transform.DOKill();
                miningTool.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f).SetLink(miningTool);
            }
        }

        private void CreateDrill()
        {
            if (drillPrefab == null) return;

            // 곡괭이 비활성화
            if (miningTool != null) miningTool.SetActive(false);

            // 플레이어 앞에 드릴 생성
            GameObject drillObj = Instantiate(drillPrefab, transform);
            // 위치 설정 (플레이어 앞 약 0.5m, 높이는 허리 정도)
            drillObj.transform.localPosition = new Vector3(0, 0.8f, 0.8f);
            drillObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            drillInstance = drillObj.GetComponent<DrillHead>();
            if (drillInstance == null) drillInstance = drillObj.AddComponent<DrillHead>();

            // 레이어 정보 전달
            drillInstance.SetTargetLayer(rockLayer);

            drillObj.SetActive(false);
        }

        private void CreateDrillCar()
        {
            if (drillCarPrefab == null) return;

            // 기존 드릴 제거
            if (drillInstance != null) Destroy(drillInstance.gameObject);

            // 드릴카 생성 (플레이어의 자식으로 설정하여 함께 이동)
            drillCarInstance = Instantiate(drillCarPrefab, transform);
            // 차량 자체의 로컬 위치는 0으로 설정 (NavMeshAgent offset이 지면 높이를 결정함)
            drillCarInstance.transform.localPosition = Vector3.zero; 
            drillCarInstance.transform.localRotation = Quaternion.identity;

            // 드릴카의 DrillHead 설정
            DrillHead carDrill = drillCarInstance.GetComponentInChildren<DrillHead>();
            if (carDrill == null) carDrill = drillCarInstance.AddComponent<DrillHead>();
            
            carDrill.SetTargetLayer(rockLayer);
            carDrill.SetActiveMining(true); 

            // 초기 상태는 비활성화 (바위 있을 때만 나타남)
            drillCarInstance.SetActive(false);
        }
    }
}
