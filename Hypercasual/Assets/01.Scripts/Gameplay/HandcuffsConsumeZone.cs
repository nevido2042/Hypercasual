using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 수갑(Handcuffs)이 실제로 시각적으로 적재되는 구역.
    /// 구역 감지 로직은 포함하지 않으며, 외부(HandcuffsDeliveryZone)에서 아이템을 넘겨받아 쌓기만 수행함.
    /// </summary>
    public class HandcuffsConsumeZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float stackHeight = 0.2f;
        [SerializeField] private Transform stackContainer;
        [SerializeField] private PrisonerQueueManager queueManager;
        [SerializeField] private float distributeInterval = 0.5f;

        private List<Transform> consumedProducts = new List<Transform>();
        private float nextDistributeTime = 0f;

        void Awake()
        {
            if (stackContainer == null)
            {
                stackContainer = new GameObject("ConsumedProducts_Container").transform;
                stackContainer.SetParent(transform);
                stackContainer.localPosition = Vector3.zero;
            }

            // 인스펙터에서 누락되었을 경우 자동으로 찾기
            if (queueManager == null)
            {
                queueManager = FindFirstObjectByType<PrisonerQueueManager>();
                if (queueManager != null) Debug.Log("[ConsumeZone] Successfully auto-assigned PrisonerQueueManager.");
            }
        }

        void Update()
        {
            // 대기열의 죄수에게 아이템 배달
            if (queueManager != null && !queueManager.IsQueueEmpty)
            {
                // 적재된 아이템이 있을 때만 로직 수행
                if (consumedProducts.Count > 0 && Time.time >= nextDistributeTime)
                {
                    Prisoner target = queueManager.GetFrontPrisoner();
                    if (target != null && !target.IsSatisfied)
                    {
                        DistributeToPrisoner(target);
                        nextDistributeTime = Time.time + distributeInterval;
                    }
                }
            }
        }

        private void DistributeToPrisoner(Prisoner prisoner)
        {
            if (consumedProducts.Count == 0) return;

            int lastIndex = consumedProducts.Count - 1;
            Transform item = consumedProducts[lastIndex];
            consumedProducts.RemoveAt(lastIndex);

            // 죄수에게 아이템 전달 (Jump 연출)
            prisoner.ReceiveHandcuff(item);
        }

        /// <summary>
        /// 외부에서 아이템을 전달받아 적재함
        /// </summary>
        public void ReceiveProduct(Transform item)
        {
            consumedProducts.Add(item);
            int index = consumedProducts.Count - 1;

            Vector3 targetLocalPos = new Vector3(0, index * stackHeight, 0);

            item.SetParent(stackContainer);
            item.DOLocalJump(targetLocalPos, 2f, 1, 0.3f).SetEase(Ease.OutQuad).OnComplete(() => {
                // 적재 완료 Pop 연출
                if (item != null) item.DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 1f);
            });
            item.DOLocalRotate(Vector3.zero, 0.3f);
        }
    }
}
