using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 컨베이어 끝에서 수압(Handcuffs)을 전달받아 한 줄로 쌓는 구역
    /// </summary>
    public class HandcuffsStackZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float stackHeight = 0.1f;      // 수직 간격
        [SerializeField] private Transform stackContainer;      // 부모 오브젝트
        [SerializeField] private float arrivalSpeed = 5.0f;     // 제품 이동 속도
        [SerializeField] private float transferInterval = 0.1f; // 플레이어에게 전달하는 시간 간격
        [SerializeField] private int maxCapacity = 10;          // 최대 적재량
        [SerializeField] private GameObject maxTextPrefab;      // "MAX" 텍스트 프리팹
        
        private float nextTransferTime = 0f;



        private List<Transform> stackedProducts = new List<Transform>();
        private float lastMaxTextTime = -1f;
        private Canvas cachedCanvas;
        private FloatingText persistentMaxText;

        public Transform ArrivalPoint => transform; // 제품이 도착할 지점
        public bool HasProducts => stackedProducts.Count > 0;
        public int ProductCount => stackedProducts.Count;
        public bool IsFull => stackedProducts.Count >= maxCapacity;

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
            if (IsFull)
            {
                // 가득 찬 경우 (방어적 코드: MachineController에서 먼저 체크함)
                ObjectPoolingManager.Instance.Release(product.gameObject);
                UpdateMaxText();
                return;
            }

            stackedProducts.Add(product);
            UpdateMaxText();
            int index = stackedProducts.Count - 1;

            // 한 줄 쌓기 위치 계산
            Vector3 targetLocalPos = new Vector3(0, index * stackHeight, 0);

            product.SetParent(stackContainer);
            
            // 도착 시 부드럽게 정렬되는 연출
            product.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutBack).OnComplete(() => {
                if (product == null) return;
                // 적재가 완료된 시점에 커졌다 원래대로 돌아오는 연출 (Pop 효과)
                product.DOKill();
                product.DOPunchScale(Vector3.one * 2f, 0.3f, 5, 1f).SetLink(product.gameObject);
            });
            product.DOLocalRotate(Vector3.zero, 0.2f).SetLink(product.gameObject);
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStack player = other.GetComponent<PlayerStack>();
                if (player != null && stackedProducts.Count > 0)
                {
                    // 일정 간격으로 전달 (여기서는 단순하게 매 프레임 체크 대신 젬스톤 배달 로직과 유사하게 처리 가능하지만 일단 단순 구현)
                    // 현재는 테스트를 위해 즉시 전달 로직 (필요시 Interval 추가 가능)
                        if (Time.time >= nextTransferTime)
                        {
                            nextTransferTime = Time.time + transferInterval;
                            
                            Transform product = TakeProduct();
                            if (product != null)
                            {
                                player.AddToFrontStack(product);
                            }
                        }
                }
            }
        }

        public Transform TakeProduct()
        {
            if (stackedProducts.Count == 0) return null;

            int lastIndex = stackedProducts.Count - 1;
            Transform product = stackedProducts[lastIndex];
            stackedProducts.RemoveAt(lastIndex);

            // 유휴 애니메이션 중지 (Kill tweens)
            product.DOKill();
            
            UpdateMaxText();
            return product;
        }

        private void UpdateMaxText()
        {
            if (IsFull)
            {
                if (persistentMaxText == null)
                {
                    if (maxTextPrefab == null) return;
                    if (cachedCanvas == null)
                    {
                        GameObject canvasObj = GameObject.FindWithTag("MainCanvas");
                        if (canvasObj != null) cachedCanvas = canvasObj.GetComponent<Canvas>();
                        if (cachedCanvas == null) cachedCanvas = Object.FindFirstObjectByType<Canvas>();
                    }
                    if (cachedCanvas == null) return;

                    Vector3 spawnWorldPos = transform.position;
                    GameObject textObj = ObjectPoolingManager.Instance.Spawn(maxTextPrefab, spawnWorldPos, Quaternion.identity, cachedCanvas.transform);
                    persistentMaxText = textObj.GetComponent<FloatingText>();
                    if (persistentMaxText != null)
                    {
                        persistentMaxText.SetupPersistent(spawnWorldPos, "MAX", Color.red);
                    }
                }
            }
            else
            {
                if (persistentMaxText != null)
                {
                    persistentMaxText.Hide();
                    persistentMaxText = null;
                }
            }
        }

    }
}
