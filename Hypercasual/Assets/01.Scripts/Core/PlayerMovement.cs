using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 플레이어의 8방향 이동 및 회전 로직 (채광 기능 분리됨)
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float velocity = 5;      // 이동 속도
        [SerializeField] private float turnSpeed = 10;    // 회전 속도

        public Vector2 InputValue => input;              // 외부 노출용 프로퍼티
        private Vector2 input;                           // 입력값 (조이스틱 또는 키보드)
        private float angle;                             // 목표 회전 각도

        private Quaternion targetRotation;               // 목표 회전값
        [SerializeField] private Transform cam;           // 카메라 참조

        FollowTarget ft;               // 카메라 팔로우 스크립트 참조

        [HideInInspector]
        [SerializeField] private VirtualJoystick joystick; // 조이스틱 인터페이스 참조

        void Start()
        {
            // 메인 카메라 및 관련 스크립트 캐싱
            cam = Camera.main.transform;
            if (cam.GetComponent<FollowTarget>())
            {
                ft = cam.GetComponent<FollowTarget>();
            }
        }

        void Update()
        {
            GetInput(); // 입력 받기 (입력 누락 방지를 위해 Update 유지)
        }

        void FixedUpdate()
        {
            // 입력이 거의 없으면 이동 로직 취소
            if (Mathf.Abs(input.x) < 0.3 && Mathf.Abs(input.y) < 0.3) return;

            CalculateDirection(); // 이동 방향 계산
            Rotate();             // 캐릭터 회전
            Move();               // 캐릭터 이동
        }

        /// <summary>
        /// 조이스틱 또는 키보드로부터 입력 벡터를 가져옴
        /// </summary>
        void GetInput()
        {
            if (joystick != null && joystick.inputVector != Vector2.zero)
            {
                input = joystick.inputVector;
            }
            else
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");
            }
        }

        /// <summary>
        /// 카메라 각도를 고려한 이동 방향 계산
        /// </summary>
        void CalculateDirection()
        {
            angle = Mathf.Atan2(input.x, input.y);
            angle = Mathf.Rad2Deg * angle;
            angle += cam.eulerAngles.y;
        }

        /// <summary>
        /// 계산된 각도로 캐릭터를 부드럽게 회전
        /// </summary>
        void Rotate()
        {
            if (ft != null && ft.CamRotation)
            {
                // 카메라 회전 방식이 활성화된 경우의 예외 처리
                transform.rotation = Quaternion.Euler(0, input.x * 1.5f, 0) * transform.rotation;
            }
            else
            {
                targetRotation = Quaternion.Euler(0, angle, 0);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 캐릭터 전방 방향으로 실제 위치 이동
        /// </summary>
        void Move()
        {
            float speedMultiplier = Mathf.Clamp01(input.magnitude);
            transform.position += transform.forward * velocity * speedMultiplier * Time.fixedDeltaTime;
        }
    }
}
