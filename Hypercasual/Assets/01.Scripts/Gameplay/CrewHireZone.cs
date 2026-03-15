using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace Hero
{
    /// <summary>
    /// 현금을 지불하여 크루를 고용하는 구역
    /// </summary>
    public class CrewHireZone : MonoBehaviour
    {
        [Header("Hire Settings")]
        [SerializeField] private int targetCashCount = 10;
        [SerializeField] private int crewSpawnCount = 1;
        [SerializeField] private float cashUnloadInterval = 0.2f;
        [SerializeField] private GameObject crewPrefab;
        [SerializeField] private Transform spawnPoint;

        [Header("UI References")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressFill;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer markerRenderer;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        private int currentCashCount = 0;
        private Coroutine payCoroutine;
        private PlayerStack playerStack;
        private Material markerMaterial;
        private bool isHiringCompleted = false;

        private void Awake()
        {
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
                SetZoneMarkerColor(inactiveColor);
            }

            if (progressFill != null)
            {
                progressFill.type = Image.Type.Filled;
                progressFill.fillMethod = Image.FillMethod.Vertical;
                progressFill.fillAmount = 0;
            }

            UpdateZoneUI();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isHiringCompleted) return;

            if (other.CompareTag("Player"))
            {
                playerStack = other.GetComponent<PlayerStack>();
                if (playerStack != null)
                {
                    SetZoneMarkerColor(activeColor);
                    if (payCoroutine != null) StopCoroutine(payCoroutine);
                    payCoroutine = StartCoroutine(HirePayRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetZoneMarkerColor(inactiveColor);
                if (payCoroutine != null)
                {
                    StopCoroutine(payCoroutine);
                    payCoroutine = null;
                }
                playerStack = null;
            }
        }

        private IEnumerator HirePayRoutine()
        {
            while (playerStack != null && currentCashCount < targetCashCount)
            {
                Transform money = playerStack.RemoveFromMoneyStack();
                if (money != null)
                {
                    currentCashCount++;
                    UpdateZoneUI();

                    money.DOMove(transform.position + Vector3.up * 0.5f, 0.2f).SetLink(money.gameObject).OnComplete(() => {
                        ObjectPoolingManager.Instance.Release(money.gameObject);
                        if (progressText != null) progressText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetLink(progressText.gameObject);
                    });

                    if (currentCashCount >= targetCashCount)
                    {
                        FinalizeHiring();
                        yield break;
                    }
                    yield return new WaitForSeconds(cashUnloadInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void UpdateZoneUI()
        {
            if (progressText != null)
            {
                progressText.text = (targetCashCount - currentCashCount).ToString();
            }

            if (progressFill != null)
            {
                float fillAmount = (float)currentCashCount / targetCashCount;
                progressFill.DOKill();
                progressFill.DOFillAmount(fillAmount, 0.2f);
            }
        }

        private void FinalizeHiring()
        {
            isHiringCompleted = true;
            
            for (int i = 0; i < crewSpawnCount; i++)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                spawnPos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                
                if (crewPrefab != null)
                {
                    GameObject crew = Instantiate(crewPrefab, spawnPos, Quaternion.identity);
                    crew.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f).SetLink(crew);
                }
            }

            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetLink(gameObject).OnComplete(() => {
                gameObject.SetActive(false);
            });
        }

        private void SetZoneMarkerColor(Color color)
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

    // Helper extension for easier SetLink if needed, but normally DOTween has it.
    // However, I used ProjectLink by mistake in the code above, fixing to SetLink.
}
