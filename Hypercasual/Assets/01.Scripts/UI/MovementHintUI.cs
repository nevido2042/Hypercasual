using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 아무런 조작이 없을 때 화면에 드래그 이동 힌트를 표시하는 UI
    /// </summary>
    public class MovementHintUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float idleThreshold = 3f; // 힌트가 나타날 때까지의 대기 시간
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Animation Settings")]
        [SerializeField] private float moveRadiusX = 120f; // 가로 이동 반경
        [SerializeField] private float moveRadiusY = 60f;  // 세로 이동 반경
        [SerializeField] private float cycleDuration = 2.5f; // 한 바퀴 도는 시간

        [Header("References")]
        [SerializeField] private CanvasGroup hintCanvasGroup;
        [SerializeField] private RectTransform fingerIcon; // 움직일 손가락 아이콘
        [SerializeField] private VirtualJoystick joystick;

        private float lastInputTime = 0f;
        private bool isHintShowing = false;

        private void Start()
        {
            if (hintCanvasGroup == null) hintCanvasGroup = GetComponent<CanvasGroup>();
            if (joystick == null) joystick = Object.FindFirstObjectByType<VirtualJoystick>();

            // 자식 오브젝트 중 "Finger"라는 이름이 있으면 자동으로 할당 시도
            if (fingerIcon == null)
            {
                Transform fingerChild = transform.Find("FingerIcon");
                if (fingerChild == null) fingerChild = transform.Find("HintImage"); // 하위 호환성
                if (fingerChild != null) fingerIcon = fingerChild.GetComponent<RectTransform>();
            }

            // 초기 상태는 숨김
            if (hintCanvasGroup != null)
            {
                hintCanvasGroup.alpha = 0f;
                // 자기 자신을 꺼버리면 Update가 멈추므로, 다른 오브젝트일 때만 SetActive(false)
                if (hintCanvasGroup.gameObject != gameObject)
                {
                    hintCanvasGroup.gameObject.SetActive(false);
                }
            }
            
            lastInputTime = Time.time;
        }

        private void Update()
        {
            // 카메라 연출 중이거나 계속하기 버튼이 떠 있다면 힌트를 보여주지 않음
            bool isCinematic = CinematicBridge.Instance != null && CinematicBridge.Instance.IsFocusing;
            bool isContinueVisible = GameContinueUI.Instance != null && GameContinueUI.Instance.IsVisible;

            if (isCinematic || isContinueVisible)
            {
                lastInputTime = Time.time;
                if (isHintShowing) HideHint();
                return;
            }

            if (CheckInput())
            {
                lastInputTime = Time.time;
                if (isHintShowing) HideHint();
            }
            else
            {
                if (!isHintShowing && Time.time - lastInputTime > idleThreshold)
                {
                    ShowHint();
                }
            }
        }

        private bool CheckInput()
        {
            // 조이스틱 입력 체크 (또는 키보드 입력 체크)
            return (joystick != null && joystick.inputVector != Vector2.zero) || 
                   Input.GetAxisRaw("Horizontal") != 0 || 
                   Input.GetAxisRaw("Vertical") != 0 ||
                   Input.GetMouseButton(0);
        }

        private void ShowHint()
        {
            isHintShowing = true;
            if (hintCanvasGroup.gameObject != gameObject)
            {
                hintCanvasGroup.gameObject.SetActive(true);
            }
            hintCanvasGroup.DOKill();
            hintCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutSine).SetLink(gameObject);

            // 1. 전체 그룹 스케일 애니메이션 (Yoyo) - 안내선과 손가락 모두에 적용됨
            hintCanvasGroup.transform.DOKill();
            hintCanvasGroup.transform.localScale = Vector3.one * 0.95f;
            hintCanvasGroup.transform.DOScale(1.05f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);

            // 2. 손가락만 무한대(∞) 모양 이동 애니메이션
            if (fingerIcon != null)
            {
                fingerIcon.DOKill();
                
                DOTween.To(() => 0f, t => {
                    // 매개변수 t (0 ~ 1)를 0 ~ 2π로 변환
                    float angle = t * Mathf.PI * 2f;
                    
                    // Lemniscate of Gerono (팔자 모양 곡선 공식)
                    // x = cos(t), y = sin(2t) / 2
                    float x = Mathf.Cos(angle) * moveRadiusX;
                    float y = (Mathf.Sin(angle * 2f) / 2f) * moveRadiusY;
                    
                    fingerIcon.localPosition = new Vector3(x, y, 0);
                }, 1f, cycleDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(fingerIcon.gameObject);
            }
        }

        private void HideHint()
        {
            isHintShowing = false;
            hintCanvasGroup.DOKill();
            hintCanvasGroup.transform.DOKill();
            if (fingerIcon != null) fingerIcon.DOKill();
            
            hintCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine).OnComplete(() =>
            {
                if (!isHintShowing && hintCanvasGroup.gameObject != gameObject)
                {
                    hintCanvasGroup.gameObject.SetActive(false);
                }
            }).SetLink(gameObject);
        }
    }
}
