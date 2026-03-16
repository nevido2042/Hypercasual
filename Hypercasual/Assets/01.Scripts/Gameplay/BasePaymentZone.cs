using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace Hero
{
    /// <summary>
    /// 현금 결제가 필요한 구역의 베이스 클래스
    /// </summary>
    public abstract class BasePaymentZone : MonoBehaviour
    {
        [Header("Payment Settings")]
        [SerializeField] protected int targetCash = 10;
        [SerializeField] protected float payInterval = 0.2f;

        [Header("Base UI References (Auto Found)")]
        protected TMP_Text progressText;
        protected Image progressFill;

        [Header("Audio")]
        [SerializeField] protected AudioClip completeSound;
        protected AudioSource audioSource;

        protected int currentCash = 0;
        protected Coroutine payCoroutine;
        protected PlayerStack playerStack;
        protected Material markerMaterial;
        protected bool isCompleted = false;

        public event System.Action OnPaymentFinished;

        protected virtual void Awake()
        {
            // UI 참조 자동 탐색
            progressText = FindComponentByName<TMP_Text>("Text (TMP)");
            progressFill = FindComponentByName<Image>("Fill");

            if (progressFill != null)
            {
                progressFill.type = Image.Type.Filled;
                progressFill.fillMethod = Image.FillMethod.Vertical;
                progressFill.fillAmount = 0;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            UpdateUI();
        }

        private T FindComponentByName<T>(string name) where T : Component
        {
            T[] components = GetComponentsInChildren<T>(true);
            foreach (var comp in components)
            {
                if (comp.name.Contains(name, System.StringComparison.OrdinalIgnoreCase))
                    return comp;
            }
            return null;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (isCompleted) return;

            if (other.CompareTag("Player"))
            {
                playerStack = other.GetComponent<PlayerStack>();
                if (playerStack != null)
                {
                    if (payCoroutine != null) StopCoroutine(payCoroutine);
                    payCoroutine = StartCoroutine(PaymentRoutine());
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (payCoroutine != null)
                {
                    StopCoroutine(payCoroutine);
                    payCoroutine = null;
                }
                playerStack = null;
            }
        }

        protected virtual IEnumerator PaymentRoutine()
        {
            while (playerStack != null && currentCash < targetCash)
            {
                Transform money = playerStack.RemoveFromMoneyStack();
                if (money != null)
                {
                    currentCash++;
                    OnPayOneCash(currentCash, targetCash);
                    UpdateUI();

                    // 돈 비행 연출
                    money.DOMove(transform.position + Vector3.up * 0.5f, 0.2f)
                        .SetLink(money.gameObject)
                        .OnComplete(() => {
                            ObjectPoolingManager.Instance.Release(money.gameObject);
                            if (progressText != null) 
                                progressText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetLink(progressText.gameObject);
                        });

                    if (currentCash >= targetCash)
                    {
                        isCompleted = true;
                        
                        // 완료 효과음 출력
                        if (audioSource != null && completeSound != null)
                        {
                            audioSource.PlayOneShot(completeSound);
                        }

                        OnPaymentComplete();
                        OnPaymentFinished?.Invoke();
                        yield break;
                    }

                    yield return new WaitForSeconds(payInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        protected virtual void UpdateUI()
        {
            if (progressText != null)
            {
                progressText.text = (targetCash - currentCash).ToString();
            }

            if (progressFill != null)
            {
                float fillAmount = (float)currentCash / targetCash;
                progressFill.DOKill();
                progressFill.DOFillAmount(fillAmount, 0.2f).SetLink(progressFill.gameObject);
            }
        }

        protected virtual void SetMarkerColor(Color color)
        {
            if (markerMaterial != null)
            {
                if (markerMaterial.HasProperty("_BaseColor"))
                    markerMaterial.SetColor("_BaseColor", color);
                else
                    markerMaterial.color = color;
            }
        }

        protected virtual void OnPayOneCash(int current, int total) { }

        protected abstract void OnPaymentComplete();
    }
}
