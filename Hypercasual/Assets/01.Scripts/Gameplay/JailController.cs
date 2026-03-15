using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    public class JailController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform doorTransform;
        [SerializeField] private TMP_Text prisonerCountText;

        [Header("Settings")]
        [SerializeField] private float openYOffset = -2.5f;
        [SerializeField] private float animationDuration = 0.6f;
        [SerializeField] private int maxCapacity = 20;

        private int prisonerCount = 0;
        private float closedY;
        private bool isDoorOpen = false;
        private bool isAnimating = false;
        private List<Prisoner> leavingPrisoners = new List<Prisoner>();

        private void Awake()
        {
            if (doorTransform != null) closedY = doorTransform.localPosition.y;
            UpdateCountText();
        }

        private void Update()
        {
            // 정기적으로 null 또는 비활성 죄수 제거
            leavingPrisoners.RemoveAll(p => p == null || !p.gameObject.activeInHierarchy);

            bool shouldBeOpen = leavingPrisoners.Count > 0;

            if (isAnimating) return;
            
            if (shouldBeOpen && !isDoorOpen)
            {
                OpenDoor();
            }
            else if (!shouldBeOpen && isDoorOpen)
            {
                CloseDoor();
            }
        }

        public void RegisterLeavingPrisoner(Prisoner p)
        {
            if (p == null) return;
            if (!leavingPrisoners.Contains(p))
            {
                leavingPrisoners.Add(p);
            }
        }

        public void UnregisterLeavingPrisoner(Prisoner p)
        {
            if (p == null) return;
            if (leavingPrisoners.Contains(p))
            {
                leavingPrisoners.Remove(p);
            }
        }

        private void OpenDoor()
        {
            if (isDoorOpen || isAnimating) return;
            isDoorOpen = true;
            isAnimating = true;
            
            if (doorTransform != null)
            {
                doorTransform.DOKill();
                doorTransform.DOLocalMoveY(closedY + openYOffset, animationDuration)
                    .SetEase(DG.Tweening.Ease.OutQuad)
                    .SetLink(doorTransform.gameObject)
                    .OnComplete(() => isAnimating = false);
            }
            else
            {
                isAnimating = false;
            }
        }

        private void CloseDoor()
        {
            if (!isDoorOpen || isAnimating) return;
            isAnimating = true;
            
            if (doorTransform != null)
            {
                doorTransform.DOKill();
                doorTransform.DOLocalMoveY(closedY, animationDuration)
                    .SetEase(DG.Tweening.Ease.InQuad)
                    .SetLink(doorTransform.gameObject)
                    .OnComplete(() => {
                        isDoorOpen = false;
                        isAnimating = false;
                    });
            }
            else
            {
                isDoorOpen = false;
                isAnimating = false;
            }
        }

        public void OnPrisonerArrived()
        {
            // 더 이상 사용되지 않음 (센서 트리거 방식에서 리스트 체크 방식으로 변경)
        }

        public void OnPrisonerEntered(GameObject prisoner)
        {
            if (prisoner == null) return;
            
            Prisoner p = prisoner.GetComponent<Prisoner>();
            if (p != null)
            {
                if (p.HasEnteredJail) return;
                p.HasEnteredJail = true;
                UnregisterLeavingPrisoner(p);
            }

            prisonerCount++;
            UpdateCountText();
        }

        private void UpdateCountText()
        {
            if (prisonerCountText != null)
            {
                prisonerCountText.text = $"{prisonerCount}/{maxCapacity}";
            }
        }
    }
}
