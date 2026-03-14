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
        
        public bool IsMining => isMining;
        public bool IsDrillUpgraded => upgradeTier >= 1; // 1단계 이상(드릴/드릴카) 여부
        public int UpgradeTier => upgradeTier;           // 현재 업그레이드 티어
        public bool IsBoardingDrillCar => upgradeTier == 2 && isMining; // 드릴카 실제 탑승 여부 extension.

        private bool isMining = false; // 현재 주변에 캘 바위가 있는지 상태
        private int upgradeTier = 0;   // 0: 곡괭이, 1: 드릴, 2: 드릴카
        private DrillHead drillInstance;      // 생성된 드릴 인스턴스 참조
        private GameObject drillCarInstance;

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
        }

        void Update()
        {
            CheckForRocks(); // 주변 바위 탐색 (애니메이션 전환용)
        }

        /// <summary>
        /// 주변에 캘 수 있는 바위가 있는지 레이어 기반으로 효율적으로 탐색
        /// </summary>
        void CheckForRocks()
        {
            isMining = false;

            // Physics.OverlapSphere를 사용하여 레이어로 필터링된 오브젝트들만 탐색 (성능 최적화)
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
            foreach (var hitCollider in hitColliders)
            {
                MineableRock rock = hitCollider.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    isMining = true;
                    break;
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
                    // 드릴카 의자 위치 (약간 높게, 뒤쪽으로)
                    characterRoot.localPosition = new Vector3(0, 1.5f, -0.6f);
                }
                else
                {
                    // 원래 위치 (지면)
                    characterRoot.localPosition = Vector3.zero;
                }
                characterRoot.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 애니메이터 타격 이벤트 시점에 호출되어 실제 바위를 채굴하는 함수
        /// </summary>
        public void PerformMiningHit()
        {
            // 드릴/드릴카 업그레이드 완료 후에는 DrillHead가 직접 채굴하므로 무시함
            if (upgradeTier >= 1) return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
            List<MineableRock> rocksInRange = new List<MineableRock>();

            // 범위 내 캘 수 있는 모든 바위 수집
            foreach (var hitCollider in hitColliders)
            {
                MineableRock rock = hitCollider.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    rocksInRange.Add(rock);
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
                
                rock.Mine(); // 바위 채굴 실행
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

            maxMineTargets += 2; // 티어 상승 시 타격 대상 크게 증가
            miningRange += 0.5f; // 사거리 증가
            
            // 시각적 피드백
            if (upgradeTier == 1 && drillInstance != null)
            {
                drillInstance.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f);
            }
            else if (upgradeTier == 2 && drillCarInstance != null)
            {
                drillCarInstance.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f);
            }
            else if (miningTool != null)
            {
                miningTool.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f);
            }
            
            Debug.Log($"[PlayerMining] Upgraded to Tier {upgradeTier}! MaxTargets: {maxMineTargets}, Range: {miningRange}");
        }

        private void CreateDrill()
        {
            if (drillPrefab == null)
            {
                Debug.LogWarning("[PlayerMining] Drill Prefab is missing!");
                return;
            }

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
            if (drillCarPrefab == null)
            {
                Debug.LogWarning("[PlayerMining] DrillCar Prefab is missing!");
                return;
            }

            // 기존 드릴 제거
            if (drillInstance != null) Destroy(drillInstance.gameObject);

            // 드릴카 생성 (플레이어의 자식으로 설정하여 함께 이동)
            drillCarInstance = Instantiate(drillCarPrefab, transform);
            // 차량이 땅에 묻히지 않도록 높이 조절
            drillCarInstance.transform.localPosition = new Vector3(0, 0.4f, 0); 
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
