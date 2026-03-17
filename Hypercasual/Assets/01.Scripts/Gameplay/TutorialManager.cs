using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Hero
{
    public enum TutorialStep
    {
        Mining,
        GemstoneDelivery,
        HandcuffsStack,
        HandcuffsDelivery,
        MoneyStack,
        Complete
    }

    /// <summary>
    /// 튜토리얼의 전체 흐름을 관리 (채굴 -> 배달 -> 수갑 수집 -> 수갑 배달 -> 현금 수집)
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TutorialMarker marker;
        public TutorialMarker Marker => marker;
        [SerializeField] private Vector3 markerOffset = new Vector3(0, 2.5f, 0);

        [Header("Zones")]
        [SerializeField] private Transform miningArea;
        [SerializeField] private Transform gemstoneDeliveryZone;
        [SerializeField] private Transform handcuffsStackZone;
        [SerializeField] private Transform handcuffsDeliveryZone;
        [SerializeField] private Transform moneyStackZone;
        [SerializeField] private MiningUpgradeZone miningUpgradeZone; 
        [SerializeField] private MinerHireZone minerHireZone;
        [SerializeField] private CrewHireZone crewHireZone;
        [SerializeField] private JailController jailController;
        [SerializeField] private JailUpgradeZone jailUpgradeZone;

        public Transform MiningArea => miningArea;
        public Transform GemstoneDeliveryZone => gemstoneDeliveryZone;
        public Transform HandcuffsStackZone => handcuffsStackZone;
        public Transform HandcuffsDeliveryZone => handcuffsDeliveryZone;
        public Transform MoneyStackZone => moneyStackZone;

        private PlayerStack playerStack;
        private FollowTarget followTarget; // 카메라 컨트롤러 참조
        private TutorialStep currentStep = TutorialStep.Mining;
        private bool isTransitioning = false;
        private Vector3 upgradeZoneOriginalScale = Vector3.one;

        public event System.Action<TutorialStep> OnStepChanged;
        public event System.Action<Transform, float, System.Action> OnCameraFocusRequest;

        private void Start()
        {
            playerStack = Object.FindFirstObjectByType<PlayerStack>();
            
            // 메인 카메라의 FollowTarget 참조
            GameObject mainCam = GameObject.FindWithTag("MainCamera");
            if (mainCam != null) followTarget = mainCam.GetComponent<FollowTarget>();

            if (playerStack != null)
            {
                // 이벤트 구독
                playerStack.OnGemstoneAdded += () => TryNextStep(TutorialStep.Mining);
                playerStack.OnGemstoneRemoved += () => TryNextStep(TutorialStep.GemstoneDelivery);
                playerStack.OnHandcuffAdded += () => TryNextStep(TutorialStep.HandcuffsStack);
                playerStack.OnHandcuffRemoved += () => TryNextStep(TutorialStep.HandcuffsDelivery);
                playerStack.OnMoneyAdded += () => TryNextStep(TutorialStep.MoneyStack);
            }

            // 시작 시 업그레이드 존들은 비활성화 상태여야 함
            if (miningUpgradeZone != null)
            {
                upgradeZoneOriginalScale = miningUpgradeZone.transform.localScale;
                miningUpgradeZone.gameObject.SetActive(false);
                miningUpgradeZone.OnFirstUpgrade += () => {
                    if (minerHireZone != null) minerHireZone.gameObject.SetActive(true);
                };
            }

            if (minerHireZone != null)
            {
                minerHireZone.gameObject.SetActive(false);
                minerHireZone.OnPaymentFinished += () => {
                    if (crewHireZone != null) crewHireZone.gameObject.SetActive(true);
                };
            }

            if (crewHireZone != null)
            {
                crewHireZone.gameObject.SetActive(false);
            }

            if (jailUpgradeZone != null)
            {
                jailUpgradeZone.gameObject.SetActive(false);
            }

            // 초기 단계 알림
            DOVirtual.DelayedCall(0.1f, () => OnStepChanged?.Invoke(currentStep)).SetLink(gameObject);
        }

        private void TryNextStep(TutorialStep requiredStep)
        {
            if (currentStep == requiredStep && !isTransitioning)
            {
                if (currentStep == TutorialStep.MoneyStack)
                {
                    StartCoroutine(ShowUpgradeZoneSequence());
                }
                else
                {
                    StartCoroutine(DelayedNextStep());
                }
            }
        }

        private System.Collections.IEnumerator DelayedNextStep()
        {
            isTransitioning = true;
            
            yield return new WaitForSeconds(1.0f);
            
            currentStep++;
            isTransitioning = false;
            OnStepChanged?.Invoke(currentStep);
        }

        /// <summary>
        /// 마지막 돈 수집 시 실행되는 연출 시퀀스
        /// </summary>
        private System.Collections.IEnumerator ShowUpgradeZoneSequence()
        {
            isTransitioning = true;

            yield return new WaitForSeconds(1.0f);

            // 2. 카메라 연출 요청 (포커싱 -> 2초 대기 -> 복구 콜백)
            if (miningUpgradeZone != null)
            {
                OnCameraFocusRequest?.Invoke(miningUpgradeZone.transform, 2.0f, () => {
                    // 연출 종료 후 처리
                    currentStep = TutorialStep.Complete;
                    isTransitioning = false;
                    OnStepChanged?.Invoke(currentStep);
                });
            }

            // 3. 업그레이드 존 활성화 및 연출 (시각적 로직만 남김)
            if (miningUpgradeZone != null)
            {
                miningUpgradeZone.gameObject.SetActive(true);
                miningUpgradeZone.transform.DOKill();
                miningUpgradeZone.transform.localScale = Vector3.zero;
                miningUpgradeZone.transform.DOScale(upgradeZoneOriginalScale, 0.8f).SetEase(Ease.OutBack).SetLink(miningUpgradeZone.gameObject);
            }
        }

        // UpdateMarker 로직은 이제 TutorialMarker 내부로 이동함 (제거 대기)
    }
}
