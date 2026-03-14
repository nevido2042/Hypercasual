using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 3D 공간 상의 오브젝트(예: 바닥 UI 마커)가 숨을 쉬듯 자연스럽게 커졌다 작아지는 연출을 담당
    /// </summary>
    public class PulsingEffect : MonoBehaviour
    {
        [Header("Pulsing Settings")]
        public float targetScaleMultiplier = 1.2f; // 목표 배율
        public float duration = 1.0f;               // 한 번 커지는 데 걸리는 시간
        public Ease easeType = Ease.InOutSine;      // 부드러운 숨쉬기 느낌을 위한 Ease

        private Vector3 initialScale;
        private Tween pulseTween;

        void Start()
        {
            initialScale = transform.localScale;
            
            // 기존 트윈이 있다면 정지
            pulseTween?.Kill();

            // 목표 스케일 계산
            Vector3 targetScale = initialScale * targetScaleMultiplier;

            // DOTween 연출: 목표 크기로 커졌다가(Yoyo) 원래대로 돌아옴을 무한 반복(-1)
            pulseTween = transform.DOScale(targetScale, duration)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            pulseTween?.Kill();
        }

        private void OnDestroy()
        {
            pulseTween?.Kill();
        }
    }
}
