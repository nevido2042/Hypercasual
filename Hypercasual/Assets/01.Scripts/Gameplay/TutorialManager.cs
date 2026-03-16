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
        [SerializeField] private Vector3 markerOffset = new Vector3(0, 2.5f, 0);

        [Header("Zones")]
        [SerializeField] private Transform miningArea;
        [SerializeField] private Transform gemstoneDeliveryZone;
        [SerializeField] private Transform handcuffsStackZone;
        [SerializeField] private Transform handcuffsDeliveryZone;
        [SerializeField] private Transform moneyStackZone;
        [SerializeField] private Transform miningUpgradeZone; // 추가: 업그레이드 존

        private PlayerStack playerStack;
        private FollowTarget followTarget; // 카메라 컨트롤러 참조
        private TutorialStep currentStep = TutorialStep.Mining;
        private bool isTransitioning = false;
        private Vector3 upgradeZoneOriginalScale = Vector3.one; // 추가: 원래 스케일 저장

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

            // 시작 시 업그레이드 존은 비활성화 상태여야 함 (수동 설정 가능성 대비)
            if (miningUpgradeZone != null)
            {
                upgradeZoneOriginalScale = miningUpgradeZone.localScale;
                miningUpgradeZone.gameObject.SetActive(false);
            }

            UpdateMarker();
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
            Debug.Log($"[Tutorial] Step {currentStep} Action Detected. Moving to next in 1s...");
            
            yield return new WaitForSeconds(1.0f);
            
            currentStep++;
            isTransitioning = false;
            UpdateMarker();
        }

        /// <summary>
        /// 마지막 돈 수집 시 실행되는 연출 시퀀스
        /// </summary>
        private System.Collections.IEnumerator ShowUpgradeZoneSequence()
        {
            isTransitioning = true;
            Debug.Log("[Tutorial] Money Collected. Showing Upgrade Zone...");

            yield return new WaitForSeconds(1.0f);

            // 1. 마커 숨기기
            if (marker != null) marker.Hide();

            // 2. 카메라 타겟을 업그레이드 존으로 이동
            Transform playerTF = playerStack != null ? playerStack.transform : null;
            if (followTarget != null && miningUpgradeZone != null)
            {
                followTarget.SetTarget(miningUpgradeZone);
            }

            // 3. 업그레이드 존 활성화 및 연출
            if (miningUpgradeZone != null)
            {
                miningUpgradeZone.gameObject.SetActive(true);
                miningUpgradeZone.DOKill();
                miningUpgradeZone.localScale = Vector3.zero;
                miningUpgradeZone.DOScale(upgradeZoneOriginalScale, 0.8f).SetEase(Ease.OutBack).SetLink(miningUpgradeZone.gameObject);
            }

            // 4. 연출 감상 대기 (2초)
            yield return new WaitForSeconds(2.0f);

            // 5. 카메라 타겟을 다시 플레이어로 복구
            if (followTarget != null && playerTF != null)
            {
                followTarget.SetTarget(playerTF);
            }

            // 6. 튜토리얼 종료
            currentStep = TutorialStep.Complete;
            isTransitioning = false;
            UpdateMarker();
            
            Debug.Log("[Tutorial] All Sequences Finished!");
        }

        private void UpdateMarker()
        {
            if (marker == null) return;

            Transform target = null;
            Vector3 offset = markerOffset;

            switch (currentStep)
            {
                case TutorialStep.Mining:
                    target = miningArea;
                    // 채굴 구역은 중앙 오프셋 계산 적용
                    RockGridGenerator grid = miningArea != null ? miningArea.GetComponent<RockGridGenerator>() : null;
                    if (grid != null) offset += grid.CenterOffset;
                    break;

                case TutorialStep.GemstoneDelivery:
                    target = gemstoneDeliveryZone;
                    break;

                case TutorialStep.HandcuffsStack:
                    target = handcuffsStackZone;
                    break;

                case TutorialStep.HandcuffsDelivery:
                    target = handcuffsDeliveryZone;
                    break;

                case TutorialStep.MoneyStack:
                    target = moneyStackZone;
                    break;

                case TutorialStep.Complete:
                    marker.Hide();
                    Debug.Log("[Tutorial] All Steps Completed!");
                    return;
            }

            if (target != null)
                marker.SetTarget(target, offset);
            else
                marker.Hide();
        }
    }
}
