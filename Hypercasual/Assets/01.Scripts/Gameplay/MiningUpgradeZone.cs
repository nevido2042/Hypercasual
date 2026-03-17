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
        [SerializeField] private int[] upgradeCosts = { 4, 10, 20 }; // 레벨별 요구 금액 (아이템 개수 기준)
        private int currentUpgradeCount = 0;

        public event System.Action OnFirstUpgrade;

        private PlayerMining playerMining;

        protected override void Awake()
        {
            // 첫 번째 레벨 비용으로 초기화
            if (upgradeCosts != null && upgradeCosts.Length > 0)
            {
                targetCash = upgradeCosts[0];
            }
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

            // 1번이라도 업그레이드하면 이벤트 발생
            if (currentUpgradeCount == 1)
            {
                OnFirstUpgrade?.Invoke();
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
                    if (upgradeCosts != null && currentUpgradeCount < upgradeCosts.Length)
                    {
                        targetCash = upgradeCosts[currentUpgradeCount];
                    }
                    currentCash = 0;
                    isCompleted = false;
                    UpdateUI();
                }).SetLink(gameObject);
            }
        }
    }
}
