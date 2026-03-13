using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    public RectTransform TouchZone;      // 터치를 감지하는 전체 화면 영역
    public RectTransform JoystickVisual; // 보이는 원형 배경
    public RectTransform HandleVisual;   // 움직이는 손잡이

    [HideInInspector]
    public Vector2 inputVector;
    
    private CanvasGroup joystickCanvasGroup;

    private void Start()
    {
        if (TouchZone == null) TouchZone = GetComponent<RectTransform>();
        
        // 처음에 조이스틱 숨기기
        joystickCanvasGroup = JoystickVisual.GetComponent<CanvasGroup>();
        if (joystickCanvasGroup == null) joystickCanvasGroup = JoystickVisual.gameObject.AddComponent<CanvasGroup>();
        
        joystickCanvasGroup.alpha = 0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 조이스틱 보이기
        joystickCanvasGroup.alpha = 1f;

        // 클릭한 위치로 조이스틱 시각 요소 이동
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(TouchZone, eventData.position, eventData.pressEventCamera, out localPos))
        {
            JoystickVisual.localPosition = localPos;
        }

        // 손잡이 초기화
        HandleVisual.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;

        // 드래그 처리
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(JoystickVisual, eventData.position, eventData.pressEventCamera, out position))
        {
            // JoystickVisual의 피벗이 (0.5, 0.5)이므로 로컬 위치는 (0,0)을 중심으로 합니다.
            // 크기의 절반으로 나누어 -1에서 1 사이의 범위를 구합니다.
            float x = position.x / (JoystickVisual.sizeDelta.x / 2f);
            float y = position.y / (JoystickVisual.sizeDelta.y / 2f);

            inputVector = new Vector2(x, y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 손잡이 이동
            HandleVisual.anchoredPosition = new Vector2(inputVector.x * (JoystickVisual.sizeDelta.x / 3f), inputVector.y * (JoystickVisual.sizeDelta.y / 3f));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 조이스틱 숨기기
        joystickCanvasGroup.alpha = 0f;

        inputVector = Vector2.zero;
        HandleVisual.anchoredPosition = Vector2.zero;
    }
}