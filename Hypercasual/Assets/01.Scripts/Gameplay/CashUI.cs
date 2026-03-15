using UnityEngine;
using TMPro;

namespace Hero
{
    public class CashUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cashText;
        [SerializeField] private PlayerStack playerStack;
        [SerializeField] private int multiplier = 5;

        private void Start()
        {
            if (playerStack == null) playerStack = Object.FindFirstObjectByType<PlayerStack>();
            if (playerStack != null) playerStack.OnMoneyStackChanged += UpdateCashDisplay;
            UpdateCashDisplay();
        }

        private void OnDestroy()
        {
            if (playerStack != null) playerStack.OnMoneyStackChanged -= UpdateCashDisplay;
        }

        private void UpdateCashDisplay()
        {
            if (cashText != null && playerStack != null)
            {
                cashText.text = (playerStack.MoneyCount * multiplier).ToString();
            }
        }
    }
}
