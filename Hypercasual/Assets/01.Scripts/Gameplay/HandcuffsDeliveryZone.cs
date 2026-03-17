using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Hero
{
    public interface IHandcuffProvider
    {
        bool HasHandcuffs();
        Transform RemoveFromFrontStack();
    }

    /// <summary>
    /// 플레이어 또는 크루의 존재를 감지하고, 수갑을 HandcuffsConsumeZone으로 전달하는 역할을 수행.
    /// </summary>
    public class HandcuffsDeliveryZone : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private HandcuffsConsumeZone consumeZone; // 실제로 수갑이 쌓일 구역

        [Header("Settings")]
        [SerializeField] private float receiveInterval = 0.15f;
        [SerializeField] private float exitGraceTime = 0.5f;
        
        [Header("Visuals")]
        [SerializeField] private MeshRenderer markerRenderer;
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private Color inactiveColor = Color.gray;

        private Coroutine consumeCoroutine;
        private Coroutine exitDelayCoroutine;
        private IHandcuffProvider currentProvider;
        private Material markerMaterial;
        private System.Collections.Generic.HashSet<Collider> _activeColliders = new System.Collections.Generic.HashSet<Collider>();

        void Awake()
        {
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
                SetColor(inactiveColor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            IHandcuffProvider provider = other.GetComponentInParent<IHandcuffProvider>();
            if (provider != null)
            {
                // 이미 추적 중인 콜라이더면 무시
                if (_activeColliders.Contains(other)) return;
                
                _activeColliders.Add(other);

                // 퇴장 대기 중이었다면 취소
                if (exitDelayCoroutine != null)
                {
                    StopCoroutine(exitDelayCoroutine);
                    exitDelayCoroutine = null;
                    return;
                }

                // 첫 번째 콜라이더 진입 시에만 초기화 및 코루틴 시작
                if (currentProvider == null)
                {
                    currentProvider = provider;
                    SetColor(activeColor);

                    // ConsumeZone에 배달자 도착 알림
                    if (consumeZone != null) consumeZone.SetDelivererInZone(true);

                    if (consumeCoroutine != null) StopCoroutine(consumeCoroutine);
                    consumeCoroutine = StartCoroutine(ConsumeRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_activeColliders.Contains(other))
            {
                _activeColliders.Remove(other);

                // 모든 콜라이더가 구역을 완전히 빠져나갔을 때만 해제 프로세스 시작 (유예 시간 적용)
                if (_activeColliders.Count == 0 && gameObject.activeInHierarchy)
                {
                    if (exitDelayCoroutine != null) StopCoroutine(exitDelayCoroutine);
                    exitDelayCoroutine = StartCoroutine(ExitDelayRoutine(other.gameObject.name));
                }
            }
        }

        private IEnumerator ExitDelayRoutine(string objectName)
        {
            yield return new WaitForSeconds(exitGraceTime);

            // 유예 시간이 지났는데도 여전히 비어있다면 진짜 퇴장 처리
            if (_activeColliders.Count == 0)
            {
                SetColor(inactiveColor);

                // ConsumeZone에 배달자 퇴장 알림
                if (consumeZone != null) consumeZone.SetDelivererInZone(false);

                if (consumeCoroutine != null)
                {
                    StopCoroutine(consumeCoroutine);
                    consumeCoroutine = null;
                }
                currentProvider = null;
            }
            
            exitDelayCoroutine = null;
        }

        private IEnumerator ConsumeRoutine()
        {
            while (currentProvider != null)
            {
                if (currentProvider.HasHandcuffs())
                {
                    if (consumeZone != null)
                    {
                        Transform item = currentProvider.RemoveFromFrontStack();
                        if (item != null)
                        {
                            consumeZone.ReceiveProduct(item);
                            yield return new WaitForSeconds(receiveInterval);
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void SetColor(Color color)
        {
            if (markerMaterial != null)
            {
                if (markerMaterial.HasProperty("_BaseColor"))
                    markerMaterial.SetColor("_BaseColor", color);
                else
                    markerMaterial.color = color;
            }
        }
    }
}
