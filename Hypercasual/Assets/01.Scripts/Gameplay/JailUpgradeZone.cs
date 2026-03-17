using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 감옥의 벽을 허물고 공간을 확장하는 업그레이드 구역
    /// </summary>
    public class JailUpgradeZone : BasePaymentZone
    {
        [Header("Jail Expansion References")]
        [SerializeField] private GameObject originalWall;    // 기존 벽
        [SerializeField] private GameObject[] objectsToActivate; // 활성화될 오브젝트들 (확장 메쉬 등)
        [SerializeField] private JailController jailController;
        [SerializeField] private int capacityMultiplier = 3;
        [SerializeField] private Transform viewTarget; // 업그레이드 전경을 비출 카메라 타겟

        private FollowTarget followTarget;

        private void Start()
        {
            // 메인 카메라의 FollowTarget 참조 미리 가져오기
            GameObject mainCam = GameObject.FindWithTag("MainCamera");
            if (mainCam != null) followTarget = mainCam.GetComponent<FollowTarget>();
        }

        protected override void OnPaymentComplete()
        {
            StartCoroutine(UpgradeSequence());
        }

        private System.Collections.IEnumerator UpgradeSequence()
        {

            // 1. 카메라 타겟 변경
            if (followTarget != null && viewTarget != null)
            {
                followTarget.SetTarget(viewTarget);
            }

            // 2. 1초 대기 (긴장감)
            yield return new WaitForSeconds(1.0f);

            // 3. 기존 업그레이드 로직 실행
            // 기존 벽 제거
            if (originalWall != null)
            {
                originalWall.SetActive(false);
            }

            // 등록된 오브젝트들 활성화 및 연출 (Pop 효과)
            if (objectsToActivate != null)
            {
                foreach (var obj in objectsToActivate)
                {
                    if (obj == null) continue;

                    Vector3 targetScale = obj.transform.localScale;
                    obj.SetActive(true);
                    
                    obj.transform.DOKill();
                    obj.transform.localScale = Vector3.zero;
                    obj.transform.DOScale(targetScale, 0.6f)
                        .SetEase(Ease.OutBack)
                        .SetLink(obj);
                }
            }

            // 수용량 증설
            if (jailController != null)
            {
                int newCapacity = jailController.MaxCapacity * capacityMultiplier;
                jailController.MaxCapacity = newCapacity;
            }

            // 4. 연출 감상 대기 (2초)
            yield return new WaitForSeconds(2.0f);

            // 5. 카메라 복구 (플레이어 찾기)
            if (followTarget != null)
            {
                PlayerStack player = Object.FindFirstObjectByType<PlayerStack>();
                if (player != null) followTarget.SetTarget(player.transform);
            }

            // 6. 업그레이드 구역 비활성화
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
                .SetLink(gameObject)
                .OnComplete(() => {
                    // 감옥 확장이 완전히 끝나면 계속하기 버튼 표시
                    if (GameContinueUI.Instance != null)
                    {
                        GameContinueUI.Instance.ShowContinueButton();
                    }
                    gameObject.SetActive(false);
                });
        }
    }
}
