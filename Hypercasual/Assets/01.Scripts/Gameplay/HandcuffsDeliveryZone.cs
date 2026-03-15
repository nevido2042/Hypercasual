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
        
        [Header("Visuals")]
        [SerializeField] private MeshRenderer markerRenderer;
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private Color inactiveColor = Color.gray;

        private Coroutine consumeCoroutine;
        private IHandcuffProvider currentProvider;
        private Material markerMaterial;

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
                currentProvider = provider;
                SetColor(activeColor);
                Debug.Log($"[HandcuffsDeliveryZone] Provider detected: {other.gameObject.name} (Parent: {other.transform.parent?.name})");

                // ConsumeZone에 배달자 도착 알림
                if (consumeZone != null) consumeZone.SetDelivererInZone(true);

                if (consumeCoroutine != null) StopCoroutine(consumeCoroutine);
                consumeCoroutine = StartCoroutine(ConsumeRoutine());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IHandcuffProvider provider = other.GetComponentInParent<IHandcuffProvider>();
            if (provider != null && provider == currentProvider)
            {
                Debug.Log($"[HandcuffsDeliveryZone] Provider left: {other.gameObject.name}");
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
                            Debug.Log($"[HandcuffsDeliveryZone] Taking handcuff from provider and delivering to ConsumeZone.");
                            consumeZone.ReceiveProduct(item);
                            yield return new WaitForSeconds(receiveInterval);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("HandcuffsDeliveryZone: target consumeZone is not assigned!");
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
