using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 현금(Cash)이 그리드 형태로 쌓이는 구역.
    /// 2열 3행(총 6칸) 그리드로 나누어 쌓음 (3줄 3줄 방식).
    /// </summary>
    public class MoneyStackZone : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject cashPrefab;

        [Header("Grid Settings")]
        public int columns = 2;              // 가로 줄 수 (2열)
        public int rows = 3;                 // 세로 줄 수 (3행) -> 3줄 3줄 구조
        public float spacingX = 0.6f;        // 가로 간격
        public float spacingZ = 0.4f;        // 세로 간격
        public float stackHeight = 0.15f;    // 층간 높이
        
        [Header("Collection Settings")]
        [SerializeField] private float collectInterval = 0.1f; // 수집 간격 (초)
        private float nextCollectTime = 0f;

        private List<Transform> stackedCash = new List<Transform>();

        /// <summary>
        /// 지정된 개수만큼 현금을 생성하고 스택으로 날림
        /// </summary>
        public void SpawnCash(int count, Vector3 startPos)
        {
            StartCoroutine(SpawnRoutine(count, startPos));
        }

        private System.Collections.IEnumerator SpawnRoutine(int count, Vector3 startPos)
        {
            for (int i = 0; i < count; i++)
            {
                if (cashPrefab == null) break;

                GameObject go = ObjectPoolingManager.Instance.Spawn(cashPrefab, startPos, Quaternion.identity);
                Transform cash = go.transform;
                
                AddCashToStack(cash);
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void AddCashToStack(Transform cash)
        {
            stackedCash.Add(cash);
            int index = stackedCash.Count - 1;

            // 2열 3행 그리드 위치 계산 (총 6자리 한 층)
            int itemsPerLevel = columns * rows;
            int level = index / itemsPerLevel;
            int levelIndex = index % itemsPerLevel;
            
            int currentRow = levelIndex / columns;
            int currentCol = levelIndex % columns;

            // X축으로 2열, Z축으로 3행 배치하여 "3줄-3줄" 느낌 구현
            float xPos = (currentCol - (columns - 1) * 0.5f) * spacingX;
            float zPos = (currentRow - (rows - 1) * 0.5f) * spacingZ;
            float yPos = level * stackHeight;

            Vector3 targetLocalPos = new Vector3(xPos, yPos, zPos);

            cash.SetParent(this.transform);
            
            // 날아가는 연출
            cash.DOKill();
            cash.DOLocalJump(targetLocalPos, 2f, 1, 0.4f)
                .SetEase(Ease.OutQuad)
                .SetLink(cash.gameObject)
                .OnComplete(() => {
                    cash.DOKill();
                    cash.DOPunchScale(Vector3.one * 0.3f, 0.2f).SetLink(cash.gameObject);
                });
            cash.DOLocalRotate(Vector3.zero, 0.4f).SetLink(cash.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStack player = other.GetComponent<PlayerStack>();
                if (player != null && stackedCash.Count > 0)
                {
                    // 수집 속도 조절
                    if (Time.time >= nextCollectTime)
                    {
                        nextCollectTime = Time.time + collectInterval;
                        
                        Transform cash = ConsumeCash();
                        if (cash != null)
                        {
                            player.AddToMoneyStack(cash);
                        }
                    }
                }
            }
        }

        private Transform ConsumeCash()
        {
            if (stackedCash.Count == 0) return null;

            int lastIndex = stackedCash.Count - 1;
            Transform cash = stackedCash[lastIndex];
            stackedCash.RemoveAt(lastIndex);

            cash.DOKill();
            return cash;
        }
    }
}
