using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightDirectionMovement : MonoBehaviour
{

    public float velocity = 5;
    public float turnSpeed = 10;

    [HideInInspector]
    public Vector2 input;
    float angle;

    [HideInInspector]
    public bool isMining = false;

    [Header("Mining Stats")]
    public float miningRange = 1.5f;     // 탐색 및 타격 반경
    public int maxMineTargets = 1;       // 한 번에 캘 수 있는 최대 바위 수
    public LayerMask rockLayer;          // 바위 오브젝트들이 속한 레이어

        Quaternion targetRotation;
        public Transform cam; // 카메라 트랜스폼

        FollowTarget ft;

        void Start()
        {
            cam = Camera.main.transform;
            if (cam.GetComponent<FollowTarget>())
            {
                ft = cam.GetComponent<FollowTarget>();
            }

        }

        void Update()
        {
            GetInput();
            CheckForRocks();

            if (Mathf.Abs(input.x) < 0.1 && Mathf.Abs(input.y) < 0.1) return;

            CalculateDirection();
            Rotate();
            Move();

        }

        [HideInInspector]
        public VirtualJoystick joystick;

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

        void CalculateDirection()
        {
            angle = Mathf.Atan2(input.x, input.y);
            angle = Mathf.Rad2Deg * angle;
            angle += cam.eulerAngles.y;
        }

        void Rotate()
        {
            if (ft != null && ft.camRotation)
            {
                transform.rotation = Quaternion.Euler(0, input.x * 1.5f, 0) * transform.rotation;
            }
            else
            {
                targetRotation = Quaternion.Euler(0, angle, 0);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

    void Move()
    {
        float speedMultiplier = Mathf.Clamp01(input.magnitude);
        transform.position += transform.forward * velocity * speedMultiplier * Time.deltaTime;
    }

    void CheckForRocks()
    {
        isMining = false;

        // 지정된 레이어(rockLayer)만 탐색하여 성능 최적화
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
        foreach (var hitCollider in hitColliders)
        {
            MineableRock rock = hitCollider.GetComponent<MineableRock>();
            if (rock != null && rock.CanBeMined)
            {
                isMining = true;
                break;
            }
        }
    }

    public void PerformMiningHit()
    {
        // 지정된 레이어만 타격 판정
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, miningRange, rockLayer);
        
        List<MineableRock> rocksInRange = new List<MineableRock>();

        foreach (var hitCollider in hitColliders)
        {
            MineableRock rock = hitCollider.GetComponent<MineableRock>();
            if (rock != null && rock.CanBeMined)
            {
                rocksInRange.Add(rock);
            }
        }

        // 플레이어와 가까운 순서대로 정렬
        rocksInRange.Sort((a, b) => 
            Vector3.Distance(transform.position, a.transform.position).CompareTo(
            Vector3.Distance(transform.position, b.transform.position)));

        // 지정된 횟수(maxMineTargets)만큼 바위를 채광
        int minedCount = 0;
        foreach (var rock in rocksInRange)
        {
            if (minedCount >= maxMineTargets) break;
            
            rock.Mine(); // 바위 채광 실행 (DOTween 애니메이션 등)
            minedCount++;
        }
    }
}
