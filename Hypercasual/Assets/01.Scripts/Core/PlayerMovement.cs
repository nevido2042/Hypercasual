using UnityEngine;
using UnityEngine.AI;

namespace Hero
{
    /// <summary>
    /// 플레이어의 8방향 이동 및 회전 로직 (NavMeshAgent 기반으로 전환)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float velocity = 8;      // 이동 속도 (5에서 8로 상향)
        [SerializeField] private float turnSpeed = 10;    // 회전 속도

        public Vector2 InputValue => input;              // 외부 노출용 프로퍼티
        private Vector2 input;                           // 입력값 (조이스틱 또는 키보드)
        private float angle;                             // 목표 회전 각도

        private Quaternion targetRotation;               // 목표 회전값
        [SerializeField] private Transform cam;           // 카메라 참조

        FollowTarget ft;               // 카메라 팔로우 스크립트 참조

        [HideInInspector]
        [SerializeField] private VirtualJoystick joystick; // 조이스틱 인터페이스 참조

        private NavMeshAgent agent;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            // 회전은 기존의 Slerp 방식을 사용하기 위해 에이전트 자동 회전은 끔
            agent.updateRotation = false;
            // 이동도 수동으로 조이스틱 값에 따라 처리함
            agent.updatePosition = true;

            // 에이전트 자체 속도 제한 해제 (스크립트의 velocity가 우선되도록)
            agent.speed = velocity * 2f;
            agent.acceleration = 1000f;
            agent.angularSpeed = 0f; // 자체 회전 사용 안 함
        }

        void Start()
        {
            // 메인 카메라 및 관련 스크립트 캐싱
            cam = Camera.main.transform;
            if (cam.GetComponent<FollowTarget>())
            {
                ft = cam.GetComponent<FollowTarget>();
            }

            // Rigidbody 설정 최적화
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            // [추가] 시작 시 플레이어를 가장 가까운 NavMesh 위로 스냅
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        void Update()
        {
            GetInput(); // 입력 받기 (입력 누락 방지를 위해 Update 유지)
        }

        void FixedUpdate()
        {
            // 입력이 거의 없으면 이동 로직 취소
            if (input.sqrMagnitude < 0.01f)
            {
                // 입력이 없을 때는 에이전트 정지
                if (agent.isOnNavMesh) agent.velocity = Vector3.zero;
                return;
            }

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
        /// NavMeshAgent를 사용한 실제 위치 이동
        /// </summary>
        void Move()
        {
            float speedMultiplier = Mathf.Clamp01(input.magnitude);
            
            // [개선] 현재 transform.forward 대신 입력 방향(angle)을 기준으로 즉각적인 이동 방향 계산
            Vector3 moveDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            // NavMeshAgent.Move는 에이전트를 현재 위치에서 일정 델타만큼 이동시키며, NavMesh 범위 내로 제한함
            if (agent.isOnNavMesh)
            {
                agent.Move(moveDir * velocity * speedMultiplier * Time.fixedDeltaTime);
            }
        }
    }
}
