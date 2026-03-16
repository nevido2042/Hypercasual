using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 플레이어의 현금을 소모하여 채광 능력을 업그레이드하는 구역
    /// </summary>
    public class MiningUpgradeZone : BasePaymentZone
    {
        [Header("Upgrade Settings")]
        [SerializeField] private int maxUpgradeLevel = 3;
        private int currentUpgradeCount = 0;

        private PlayerMining playerMining;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerMining = other.GetComponent<PlayerMining>();
            }
            base.OnTriggerEnter(other);
        }

        protected override void OnPaymentComplete()
        {
            if (playerMining != null)
            {
                playerMining.UpgradeMining();
                currentUpgradeCount++;
            }

            // 업그레이드 완료 연출
            if (progressFill != null)
            {
                progressFill.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(progressFill.gameObject);
            }

            if (currentUpgradeCount >= maxUpgradeLevel)
            {
                // 최종 업그레이드 완료 시 구역 비활성화
                transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack)
                    .SetLink(gameObject)
                    .OnComplete(() => {
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                // 다음 단계를 위해 초기화
                DOVirtual.DelayedCall(0.5f, () => {
                    currentCash = 0;
                    isCompleted = false;
                    UpdateUI();
                }).SetLink(gameObject);
            }
        }
    }
}
