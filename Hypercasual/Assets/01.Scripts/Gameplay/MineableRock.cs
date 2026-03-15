using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace Hero
{
    /// <summary>
    /// 채굴 가능한 바위의 데이터와 시각적 연출 관리
    /// </summary>
    public class MineableRock : MonoBehaviour
    {
        [Header("드랍 설정")]
        [SerializeField] private GameObject gemstonePrefab; // 채굴 시 생성될 젬스톤 프리팹

        [Header("파티클 설정")]
        [SerializeField] private GameObject hitParticlePrefab; // 여기에 HitRock02 프리팹을 할당하세요

        [Header("밸런스 설정")]
        [SerializeField] private float respawnTime = 5.0f; // 바위가 다시 생성되는 시간 (초)

        private Vector3 originalScale; // 원래 크기 저장용
        public bool CanBeMined { get; private set; } = true; // 채굴 가능 여부
        private Collider col;
        private Renderer[] renderers;

        void Start()
        {
            originalScale = transform.localScale;
            col = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
        }

        /// <summary>
        /// 바위 채굴 실행 (채광 주체에 따라 드랍 위치 변경)
        /// </summary>
        public void Mine(GameObject miner = null)
        {
            if (!CanBeMined) return;
            CanBeMined = false;
            
            // 충돌 감지 비활성화
            if (col != null) col.enabled = false;

            // 젬스톤 생성 및 전달
            if (gemstonePrefab != null)
            {
                // 인스턴스 생성 후 해당 주체에게 전달
                GameObject gemObj = Instantiate(gemstonePrefab, transform.position, Quaternion.identity);
                
                // 1. 광부가 캔 경우 -> 배달 구역으로
                if (miner != null && miner.CompareTag("Miner"))
                {
                    GemstoneDeliveryZone deliveryZone = Object.FindFirstObjectByType<GemstoneDeliveryZone>();
                    if (deliveryZone != null)
                    {
                        deliveryZone.DeliverGem(gemObj.transform);
                    }
                    else
                    {
                        // 배달 구역이 없으면 플레이어에게 (백업)
                        PlayerStack stack = Object.FindFirstObjectByType<PlayerStack>();
                        if (stack != null) stack.AddToStack(gemObj);
                    }
                }
                // 2. 플레이어가 캔 경우 (또는 기본) -> 플레이어 스택으로
                else
                {
                    PlayerStack stack = Object.FindFirstObjectByType<PlayerStack>();
                    if (stack != null)
                    {
                        stack.AddToStack(gemObj);
                    }
                }
            }
            
            // 파티클 생성 (오브젝트 풀링 사용)
            if (hitParticlePrefab != null)
            {
                EffectManager.Instance.Spawn(hitParticlePrefab, transform.position, Quaternion.identity);
            }

            // DOTween 작아지는 애니메이션 대신 즉시 렌더러를 꺼서 아예 안 보이게 처리
            if (renderers != null)
            {
                foreach (var r in renderers) r.enabled = false;
            }

            // 채굴 완료 후 리스폰 코루틴 시작
            StartCoroutine(RespawnRoutine());
        }

        /// <summary>
        /// 일정 시간 뒤 바위를 다시 생성하는 로직
        /// </summary>
        IEnumerator RespawnRoutine()
        {
            // 설정된 리스폰 시간만큼 대기
            yield return new WaitForSeconds(respawnTime);
            
            // 상태 복구 및 충돌 활성화
            if (col != null) col.enabled = true;
            CanBeMined = true;
            
            // 렌더러 다시 켜기
            if (renderers != null)
            {
                foreach (var r in renderers) r.enabled = true;
            }

            // 등장 연출을 위해 크기를 0으로 만들고 커지게 함
            transform.localScale = Vector3.zero;
            // DOTween: 원래 크기로 튀어오르며 나타남(OutBack)
            transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
        }
    }
}
