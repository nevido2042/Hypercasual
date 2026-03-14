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
        public bool IsMining => isMining;
        private bool isMining = false; // 현재 주변에 캘 바위가 있는지 상태

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

            // 가시성 업데이트 (상태 변화가 있을 때만 처리해도 되지만 안정성을 위해 체크)
            if (miningTool != null)
            {
                if (isMining != miningTool.activeSelf)
                {
                    miningTool.SetActive(isMining);
                }
            }
        }

        /// <summary>
        /// 애니메이터 타격 이벤트 시점에 호출되어 실제 바위를 채굴하는 함수
        /// </summary>
        public void PerformMiningHit()
        {
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

            // maxMineTargets 수만큼 순차적으로 채광 처리 (드릴 등 광역 도구 대응)
            int minedCount = 0;
            foreach (var rock in rocksInRange)
            {
                if (minedCount >= maxMineTargets) break;
                
                rock.Mine(); // 바위 채굴 실행 (연출 실행)
                minedCount++;
            }
        }

        /// <summary>
        /// 채광 능력 업그레이드
        /// </summary>
        public void UpgradeMining()
        {
            maxMineTargets++;
            miningRange += 0.2f; // 업그레이드 시 사거리도 약간 증가
            
            // 시각적 피드백 (곡괭이 잠깐 커짐)
            if (miningTool != null)
            {
                miningTool.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f);
            }
            
            Debug.Log($"[PlayerMining] Upgraded! MaxTargets: {maxMineTargets}, Range: {miningRange}");
        }
    }
}
