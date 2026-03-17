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

                // 퇴장 유예 중이었다면 취소
                if (exitDelayCoroutine != null)
                {
                    StopCoroutine(exitDelayCoroutine);
                    exitDelayCoroutine = null;
                }

                // 구역 활성화 처리
                SetColor(activeColor);

                // ConsumeZone에 배달자 도착 알림
                if (consumeZone != null) consumeZone.SetDelivererInZone(true);

                if (consumeCoroutine == null)
                {
                    consumeCoroutine = StartCoroutine(ConsumeRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_activeColliders.Contains(other))
            {
                _activeColliders.Remove(other);

                // 구역이 완전히 비었을 때 유예 시간을 두고 종료 처리
                if (_activeColliders.Count == 0 && gameObject.activeInHierarchy)
                {
                    if (exitDelayCoroutine != null) StopCoroutine(exitDelayCoroutine);
                    exitDelayCoroutine = StartCoroutine(ExitDelayRoutine());
                }
            }
        }

        private IEnumerator ExitDelayRoutine()
        {
            yield return new WaitForSeconds(exitGraceTime);

            // 유예 시간이 지났는데도 여전히 비어있다면 진짜 비활성화
            if (_activeColliders.Count == 0)
            {
                SetColor(inactiveColor);
                if (consumeZone != null) consumeZone.SetDelivererInZone(false);
                
                if (consumeCoroutine != null)
                {
                    StopCoroutine(consumeCoroutine);
                    consumeCoroutine = null;
                }
            }
            
            exitDelayCoroutine = null;
        }

        private IEnumerator ConsumeRoutine()
        {
            while (_activeColliders.Count > 0)
            {
                bool producedAny = false;

                // 구역 내의 모든 제공자로부터 순차적으로 수집
                // 순회 중 OnTriggerExit 등에 의해 컬렉션이 수정되는 것을 방지하기 위해 복사본 생성
                var colliders = new System.Collections.Generic.List<Collider>(_activeColliders);
                foreach (var collider in colliders)
                {
                    // 이미 구역을 나갔거나 파괴된 경우 제외
                    if (collider == null || !_activeColliders.Contains(collider)) continue;
                    
                    IHandcuffProvider provider = collider.GetComponentInParent<IHandcuffProvider>();
                    if (provider != null && provider.HasHandcuffs())
                    {
                        if (consumeZone != null)
                        {
                            Transform item = provider.RemoveFromFrontStack();
                            if (item != null)
                            {
                                consumeZone.ReceiveProduct(item);
                                producedAny = true;
                                yield return new WaitForSeconds(receiveInterval);
                            }
                        }
                    }
                }

                if (!producedAny) yield return null;
            }
            consumeCoroutine = null;
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
