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
        // 주변 1.5 반경 내의 콜라이더 탐색
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            string objName = hitCollider.gameObject.name.ToLower();
            if (objName.Contains("rock") || hitCollider.CompareTag("Rock"))
            {
                isMining = true;
                break;
            }
        }
    }
}
