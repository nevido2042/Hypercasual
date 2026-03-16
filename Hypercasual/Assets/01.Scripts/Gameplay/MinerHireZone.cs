using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 현금을 지불하여 광부를 고용하는 구역
    /// </summary>
    public class MinerHireZone : BasePaymentZone
    {
        [Header("Hire Specific Settings")]
        [SerializeField] private int minerSpawnCount = 3;
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private Transform spawnPoint;

        protected override void OnPaymentComplete()
        {
            for (int i = 0; i < minerSpawnCount; i++)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                spawnPos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                
                if (minerPrefab != null)
                {
                    GameObject miner = Instantiate(minerPrefab, spawnPos, Quaternion.identity);
                    miner.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f).SetLink(miner);
                }
            }

            transform.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
                .SetLink(gameObject)
                .OnComplete(() => {
                    gameObject.SetActive(false);
                });
        }
    }
}
