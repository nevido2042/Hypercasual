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
        public GameObject gemstonePrefab; // 채굴 시 생성될 젬스톤 프리팹

        private Vector3 originalScale; // 원래 크기 저장용
        public bool CanBeMined { get; private set; } = true; // 채굴 가능 여부
        private Collider col;

        void Start()
        {
            originalScale = transform.localScale;
            col = GetComponent<Collider>();
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
            
            // DOTween: 크기를 0으로 축소하며 사라짐
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                // 채굴 완료 후 리스폰 코루틴 시작
                StartCoroutine(RespawnRoutine());
            });
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
            
            // DOTween: 원래 크기로 튀어오르며 나타남(OutBack)
            transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
        }
    }
}
