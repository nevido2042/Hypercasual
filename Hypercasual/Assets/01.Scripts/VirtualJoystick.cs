using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hero
{
    /// <summary>
    /// 모바일 UI 가상 조이스틱 핸들러
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("UI 참조")]
        public RectTransform TouchZone;      // 터치 인식 영역
        public RectTransform JoystickVisual; // 조이스틱 배경 시각 요소
        public RectTransform HandleVisual;   // 조이스틱 핸들 시각 요소

        [HideInInspector]
        public Vector2 inputVector; // 캐릭터 이동에 사용할 입력 벡터
        
        private CanvasGroup joystickCanvasGroup;

        private void Start()
        {
            if (TouchZone == null) TouchZone = GetComponent<RectTransform>();
            
            joystickCanvasGroup = JoystickVisual.GetComponent<CanvasGroup>();
            if (joystickCanvasGroup == null) joystickCanvasGroup = JoystickVisual.gameObject.AddComponent<CanvasGroup>();
            
            // 시작 시 조이스틱 숨김
            joystickCanvasGroup.alpha = 0f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 터치 시 조이스틱 표시
            joystickCanvasGroup.alpha = 1f;

            // 터치한 위치로 조이스틱 배경 이동
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(TouchZone, eventData.position, eventData.pressEventCamera, out localPos))
            {
                JoystickVisual.localPosition = localPos;
            }

            HandleVisual.anchoredPosition = Vector2.zero;
            inputVector = Vector2.zero;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            // 조이스틱 배경 기준으로 핸들 위치 계산
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(JoystickVisual, eventData.position, eventData.pressEventCamera, out position))
            {
                float x = position.x / (JoystickVisual.sizeDelta.x / 2f);
                float y = position.y / (JoystickVisual.sizeDelta.y / 2f);

                inputVector = new Vector2(x, y);
                // 입력 벡터 크기를 1로 제한
                inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

                // 핸들 시각 요소 위치 업데이트
                HandleVisual.anchoredPosition = new Vector2(inputVector.x * (JoystickVisual.sizeDelta.x / 3f), inputVector.y * (JoystickVisual.sizeDelta.y / 3f));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 터치 해제 시 조이스틱 숨김 및 값 초기화
            joystickCanvasGroup.alpha = 0f;
            inputVector = Vector2.zero;
            HandleVisual.anchoredPosition = Vector2.zero;
        }
    }
}