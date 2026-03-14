using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 플레이어의 존재를 감지하고, 플레이어가 가진 수갑을 HandcuffsConsumeZone으로 전달하는 역할을 수행.
    /// </summary>
    public class HandcuffsDeliveryZone : MonoBehaviour
    {
        [Header("Target")]
        public HandcuffsConsumeZone consumeZone; // 실제로 수갑이 쌓일 구역

        [Header("Settings")]
        public float receiveInterval = 0.15f;
        
        [Header("Visuals")]
        public MeshRenderer markerRenderer;
        public Color activeColor = Color.yellow;
        public Color inactiveColor = Color.gray;

        private Coroutine consumeCoroutine;
        private PlayerStack playerStack;
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
            if (other.CompareTag("Player"))
            {
                playerStack = other.GetComponent<PlayerStack>();
                if (playerStack != null)
                {
                    SetColor(activeColor);
                    if (consumeCoroutine != null) StopCoroutine(consumeCoroutine);
                    consumeCoroutine = StartCoroutine(ConsumeRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetColor(inactiveColor);
                if (consumeCoroutine != null)
                {
                    StopCoroutine(consumeCoroutine);
                    consumeCoroutine = null;
                }
                playerStack = null;
            }
        }

        private IEnumerator ConsumeRoutine()
        {
            while (playerStack != null)
            {
                if (playerStack.HasHandcuffs())
                {
                    if (consumeZone != null)
                    {
                        Transform item = playerStack.RemoveFromFrontStack();
                        if (item != null)
                        {
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
