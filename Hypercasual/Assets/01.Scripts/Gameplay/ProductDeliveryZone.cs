using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 컨베이어 끝에서 수압(Handcuffs)을 전달받아 한 줄로 쌓는 구역
    /// </summary>
    public class ProductDeliveryZone : MonoBehaviour
    {
        [Header("Settings")]
        public float stackHeight = 0.2f;      // 수직 간격
        public Transform stackContainer;      // 부모 오브젝트
        public float arrivalSpeed = 5.0f;     // 제품 이동 속도

        private List<Transform> stackedProducts = new List<Transform>();

        public Transform ArrivalPoint => transform; // 제품이 도착할 지점

        void Awake()
        {
            if (stackContainer == null)
            {
                stackContainer = new GameObject("StackedProducts").transform;
                stackContainer.SetParent(transform);
                stackContainer.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 머신에서 생산된 제품을 이 구역으로 이동시키도록 설정
        /// </summary>
        public void RegisterProduct(GameObject product)
        {
            MovingProduct mp = product.GetComponent<MovingProduct>();
            if (mp == null) mp = product.AddComponent<MovingProduct>();

            mp.Setup(ArrivalPoint, arrivalSpeed, OnProductArrived);
        }

        private void OnProductArrived(Transform product)
        {
            stackedProducts.Add(product);
            int index = stackedProducts.Count - 1;

            // 한 줄 쌓기 위치 계산
            Vector3 targetLocalPos = new Vector3(0, index * stackHeight, 0);

            product.SetParent(stackContainer);
            
            // 도착 시 부드럽게 정렬되는 연출
            product.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutBack).OnComplete(() => {
                // 적재가 완료된 시점에 커졌다 원래대로 돌아오는 연출 (Pop 효과)
                product.DOPunchScale(Vector3.one * 2f, 0.3f, 5, 1f);
            });
            product.DOLocalRotate(Vector3.zero, 0.2f);
        }
    }
}
