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

        protected override void OnPaymentComplete()
        {
            // 1. 기존 벽 제거
            if (originalWall != null)
            {
                originalWall.SetActive(false);
            }

            // 2. 등록된 오브젝트들 활성화 및 연출 (Pop 효과)
            if (objectsToActivate != null)
            {
                foreach (var obj in objectsToActivate)
                {
                    if (obj == null) continue;

                    // 팝 연출 적용
                    Vector3 targetScale = obj.transform.localScale;
                    obj.SetActive(true);
                    
                    obj.transform.DOKill();
                    obj.transform.localScale = Vector3.zero;
                    obj.transform.DOScale(targetScale, 0.6f)
                        .SetEase(Ease.OutBack)
                        .SetLink(obj);
                }
            }

            // 3. 수용량 증설
            if (jailController != null)
            {
                int newCapacity = jailController.MaxCapacity * capacityMultiplier;
                jailController.MaxCapacity = newCapacity;
                Debug.Log($"[JailUpgradeZone] Jail Expanded! New Capacity: {newCapacity}");
            }

            // 4. 업그레이드 구역 비활성화
            transform.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
                .SetLink(gameObject)
                .OnComplete(() => {
                    gameObject.SetActive(false);
                });
        }
    }
}
