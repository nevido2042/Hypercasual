using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace Hero
{
    /// <summary>
    /// 플레이어의 현금을 소모하여 채광 능력을 업그레이드하는 구역
    /// </summary>
    public class MiningUpgradeZone : MonoBehaviour
    {
        [Header("Upgrade Settings")]
        [SerializeField] private int targetCash = 5;       // 필요한 현금 총액
        [SerializeField] private float unloadInterval = 0.2f; // 현금 소모 간격
        
        [Header("UI References")]
        [SerializeField] private TMP_Text progressText;    // "1 / 5" 표시용 텍스트
        [SerializeField] private Image progressFill;     // 사각형 Bar 형태의 게이지
        
        [Header("Visuals")]
        [SerializeField] private MeshRenderer markerRenderer;
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color gaugeColor = Color.green; // 게이지 색상

        private int currentCash = 0;
        private Coroutine upgradeCoroutine;
        private PlayerStack playerStack;
        private PlayerMining playerMining;
        private Material markerMaterial;

        void Awake()
        {
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
                SetMarkerColor(inactiveColor);
            }

            // 게이지 색상 및 타입 설정
            if (progressFill != null)
            {
                progressFill.type = Image.Type.Filled;
                progressFill.fillMethod = Image.FillMethod.Vertical;
                progressFill.color = gaugeColor;
                progressFill.fillAmount = 0;
            }

            UpdateUI();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerStack = other.GetComponent<PlayerStack>();
                playerMining = other.GetComponent<PlayerMining>();

                if (playerStack != null && playerMining != null)
                {
                    SetMarkerColor(activeColor);
                    if (upgradeCoroutine != null) StopCoroutine(upgradeCoroutine);
                    upgradeCoroutine = StartCoroutine(UpgradeRoutine());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetMarkerColor(inactiveColor);
                if (upgradeCoroutine != null)
                {
                    StopCoroutine(upgradeCoroutine);
                    upgradeCoroutine = null;
                }
                playerStack = null;
            }
        }

        private IEnumerator UpgradeRoutine()
        {
            while (playerStack != null && currentCash < targetCash)
            {
                Transform money = playerStack.RemoveFromMoneyStack();
                if (money != null)
                {
                    currentCash++;
                    UpdateUI();

                    // 돈이 소모되는 연출 (구역 중앙으로 날아감)
                    money.DOMove(transform.position + Vector3.up * 0.5f, 0.2f).OnComplete(() => {
                        Destroy(money.gameObject);
                        // UI에 살짝 펄스 효과
                        if (progressText != null) progressText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
                    });

                    if (currentCash >= targetCash)
                    {
                        CompleteUpgrade();
                        yield break;
                    }

                    yield return new WaitForSeconds(unloadInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void UpdateUI()
        {
            if (progressText != null)
            {
                // "현재 / 목표" 대신 "남은 금액"만 표시
                progressText.text = (targetCash - currentCash).ToString();
            }

            if (progressFill != null)
            {
                float fillAmount = (float)currentCash / targetCash;
                progressFill.DOKill();
                progressFill.DOFillAmount(fillAmount, 0.2f);
            }
        }

        private void CompleteUpgrade()
        {
            if (playerMining != null)
            {
                playerMining.UpgradeMining();
            }

            // 업그레이드 완료 연출 (반짝거림)
            if (progressFill != null)
            {
                progressFill.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
            
            DOVirtual.DelayedCall(0.5f, () => {
                currentCash = 0;
                UpdateUI();
            });
        }

        private void SetMarkerColor(Color color)
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
