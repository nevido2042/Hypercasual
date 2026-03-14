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
        /// 바위 채굴 실행 (DOTween 애니메이션 포함)
        /// </summary>
        public void Mine()
        {
            if (!CanBeMined) return;
            CanBeMined = false;
            
            // 충돌 감지 비활성화
            if (col != null) col.enabled = false;

            // 젬스톤 생성 요청 (PlayerStack에게 위임)
            if (gemstonePrefab != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Hero.PlayerStack stack = player.GetComponent<Hero.PlayerStack>();
                    if (stack != null)
                    {
                        stack.AddToStack(gemstonePrefab);
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
            // 1초 대기
            yield return new WaitForSeconds(1.0f);
            
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
