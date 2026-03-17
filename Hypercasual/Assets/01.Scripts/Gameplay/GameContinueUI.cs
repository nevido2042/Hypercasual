using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 감옥 확장 완료 후 게임을 계속 이어갈 수 있게 해주는 UI 클래스
    /// </summary>
    public class GameContinueUI : MonoBehaviour
    {
        public static GameContinueUI Instance { get; private set; }

        [SerializeField] private GameObject continuePanel;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (continuePanel != null) continuePanel.SetActive(false);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
        }

        public void ShowContinueButton()
        {
            if (continuePanel != null)
            {
                continuePanel.SetActive(true);
                continuePanel.transform.DOKill();
                continuePanel.transform.localScale = Vector3.zero;
                continuePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }
        }

        private void ContinueGame()
        {
            Debug.Log("[GameContinueUI] Continuing game...");
            
            if (continuePanel != null)
            {
                continuePanel.transform.DOKill();
                continuePanel.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => continuePanel.SetActive(false));
            }
        }
    }
}
