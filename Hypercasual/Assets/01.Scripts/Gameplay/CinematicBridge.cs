using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// TutorialManager의 이벤트를 받아 카메라 연출(포커싱)을 수행하는 브릿지 클래스
    /// </summary>
    public class CinematicBridge : MonoBehaviour
    {
        private TutorialManager _manager;
        private FollowTarget _followTarget;
        private Transform _playerTransform;

        private void Awake()
        {
            _manager = Object.FindFirstObjectByType<TutorialManager>();
            
            GameObject mainCam = GameObject.FindWithTag("MainCamera");
            if (mainCam != null) _followTarget = mainCam.GetComponent<FollowTarget>();

            PlayerStack player = Object.FindFirstObjectByType<PlayerStack>();
            if (player != null) _playerTransform = player.transform;
        }

        private void Start()
        {
            if (_manager != null)
            {
                _manager.OnCameraFocusRequest += HandleCameraFocus;
            }
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnCameraFocusRequest -= HandleCameraFocus;
            }
        }

        private void HandleCameraFocus(Transform target, float duration, System.Action onComplete)
        {
            if (_followTarget == null) return;

            StartCoroutine(FocusRoutine(target, duration, onComplete));
        }

        private System.Collections.IEnumerator FocusRoutine(Transform target, float duration, System.Action onComplete)
        {
            // 1. 타겟 포커싱
            _followTarget.SetTarget(target);

            // 2. 지정된 시간만큼 대기
            yield return new WaitForSeconds(duration);

            // 3. 다시 플레이어로 복구
            if (_playerTransform != null)
            {
                _followTarget.SetTarget(_playerTransform);
            }

            // 4. 완료 알림
            onComplete?.Invoke();
        }
    }
}
