using UnityEngine;
using TMPro;

namespace Hero
{
    /// <summary>
    /// 대기열 맨 앞의 죄수 머리 위에 스크린 공간 UI를 표시
    /// </summary>
    public class PrisonerQueueUI : MonoBehaviour
    {
        [Header("References")]
        public PrisonerQueueManager queueManager;
        public RectTransform uiContainer;       // UI 요소
        public TMP_Text requirementText;        // 수치를 표시할 텍스트

        [Header("Settings")]
        public Vector3 worldOffset = new Vector3(0, 2.5f, 0);

        private Camera mainCamera;

        void Awake()
        {
            mainCamera = Camera.main;
            if (uiContainer != null) uiContainer.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (queueManager == null || uiContainer == null || mainCamera == null || requirementText == null) return;

            Prisoner frontPrisoner = queueManager.GetFrontPrisoner();

            if (frontPrisoner == null || frontPrisoner.IsSatisfied)
            {
                if (uiContainer.gameObject.activeSelf) uiContainer.gameObject.SetActive(false);
                return;
            }

            if (!uiContainer.gameObject.activeSelf) uiContainer.gameObject.SetActive(true);
            
            int remaining = frontPrisoner.RemainingCount;
            requirementText.text = remaining > 0 ? remaining.ToString() : "OK!";
            requirementText.color = remaining > 0 ? Color.black : Color.green;

            if (frontPrisoner.transform == null) return;

            Vector3 worldPos = frontPrisoner.transform.position + worldOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0)
            {
                uiContainer.gameObject.SetActive(false);
            }
            else
            {
                uiContainer.position = screenPos;
            }
        }
    }
}
