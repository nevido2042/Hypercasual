using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 플레이어가 진입하면 젬스톤을 전달받아 2줄로 쌓는 구역
    /// </summary>
    public class GemstoneDeliveryZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float unloadInterval = 0.1f;    // 젬스톤 전달 간격
        [SerializeField] private float stackSpacing = 0.5f;     // 가로 간격 (2줄 사이)
        [SerializeField] private float stackHeight = 0.2f;      // 세로 높이 간격
        [SerializeField] private Transform stackContainer;      // 젬스톤이 쌓일 부모 오브젝트

        [Header("Visuals")]
        [SerializeField] private MeshRenderer markerRenderer;   // 구역 마커 렌더러
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.blue;

        private List<Transform> deliveredGems = new List<Transform>();
        private Coroutine unloadCoroutine;
        private PlayerStack playerStack;
        private Material markerMaterial;

        void Awake()
        {
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
                markerMaterial.color = inactiveColor;
            }

            if (stackContainer == null)
            {
                stackContainer = new GameObject("DeliveredGems").transform;
                stackContainer.SetParent(transform);
                stackContainer.localPosition = Vector3.zero;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerStack = other.GetComponent<PlayerStack>();
                if (playerStack != null)
                {
                // 마커 색상 변경
                if (unloadCoroutine != null) StopCoroutine(unloadCoroutine);
                SetMarkerColor(activeColor);
                unloadCoroutine = StartCoroutine(UnloadRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 마커 색상 복구
                SetMarkerColor(inactiveColor);

                // 배달 중지
                if (unloadCoroutine != null)
                {
                    StopCoroutine(unloadCoroutine);
                    unloadCoroutine = null;
                }
                playerStack = null;
            }
        }

        private IEnumerator UnloadRoutine()
        {
            while (playerStack != null)
            {
                Transform gem = playerStack.RemoveFromStack();
                if (gem != null)
                {
                    DeliverGem(gem);
                    yield return new WaitForSeconds(unloadInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void DeliverGem(Transform gem)
        {
            deliveredGems.Add(gem);
            int index = deliveredGems.Count - 1;

            // 2줄 쌓기 위치 계산
            int row = index / 2;
            int col = index % 2;

            float xPos = (col == 0) ? -stackSpacing * 0.5f : stackSpacing * 0.5f;
            float yPos = row * stackHeight;
            Vector3 targetLocalPos = new Vector3(xPos, yPos, 0);

            // 배달 연출: 부모 설정 및 이동
            gem.SetParent(stackContainer);
            
            // 크기가 0이거나 작아져 있을 수 있으므로 초기화 (머신에서 소모될 때 0이 되기 때문)
            gem.localScale = Vector3.one; 
            
            // DOTween으로 점프하듯 이동하는 연출
            gem.DOLocalJump(targetLocalPos, 2f, 1, 0.3f).SetEase(Ease.OutQuad).OnComplete(() => {
                // 적재가 완료된 시점에 커졌다 원래대로 돌아오는 연출 (Pop 효과)
                gem.DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 1f);
            });
            gem.DOLocalRotate(Vector3.zero, 0.3f);
        }

        private void SetMarkerColor(Color color)
        {
            if (markerMaterial != null)
            {
                // URP Lit 셰이더와 Standard 셰이더 모두 대응하도록 처리
                if (markerMaterial.HasProperty("_BaseColor"))
                    markerMaterial.SetColor("_BaseColor", color);
                else
                    markerMaterial.color = color;
            }
        }

        /// <summary>
        /// 구역에 쌓여있는 젬스톤이 있는지 확인
        /// </summary>
        public bool HasGem() => deliveredGems.Count > 0;

        /// <summary>
        /// 구역에서 젬스톤 하나를 소모하여 반환
        /// </summary>
        public Transform ConsumeGem()
        {
            if (deliveredGems.Count == 0) return null;

            int lastIndex = deliveredGems.Count - 1;
            Transform gem = deliveredGems[lastIndex];
            deliveredGems.RemoveAt(lastIndex);
            
            return gem;
        }
    }
}
