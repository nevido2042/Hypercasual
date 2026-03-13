using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 대상을 따라다니는 메인 카메라 컨트롤러
    /// </summary>
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;        // 따라갈 대상
        public Vector3 offsetPos;       // 대상과의 거리 간격
        public float moveSpeed = 5;     // 위치 이동 속도
        public float turnSpeed = 10;    // 회전 속도
        public float smoothSpeed = 0.5f; // 부드러운 전환 속도

        public bool camRotation;        // 카메라 회전 모드 여부

        Quaternion targetRotation;
        Vector3 targetPos;
        bool smoothRotating = false;

        void Update()
        {
            if (!camRotation)
            {
                // 일반적인 쿼터뷰 모드
                MoveWithTarget();
                LookAtTarget();
            }
            else
            {
                // 캐릭터 회전 동기화 모드
                LookatRotation();
            }

            // G/H 키를 통해 씬 내에서 카메라 각도 조절 테스트 가능
            if (Input.GetKeyDown(KeyCode.G) && !smoothRotating)
            {
                StartCoroutine("RotateAroundTarget", 45);
            }

            if (Input.GetKeyDown(KeyCode.H) && !smoothRotating)
            {
                StartCoroutine("RotateAroundTarget", -45);
            }
        }

        void MoveWithTarget()
        {
            if (!target) return;
            targetPos = target.transform.position + offsetPos;
            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }

        void LookAtTarget()
        {
            if (!target) return;
            targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        void LookatRotation()
        {
            if (!target) return;

            float wantedRotationAngle = target.eulerAngles.y;
            float wantedHeight = target.position.y + offsetPos.y;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, smoothSpeed * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, moveSpeed * Time.deltaTime);

            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            transform.position = target.position;
            transform.position -= currentRotation * Vector3.forward * -offsetPos.z;
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            transform.LookAt(target);
        }

        System.Collections.IEnumerator RotateAroundTarget(float angle)
        {
            Vector3 vel = Vector3.zero;
            Vector3 targerOffsetPos = Quaternion.Euler(0, angle, 0) * offsetPos;
            float dist = Vector3.Distance(offsetPos, targerOffsetPos);

            smoothRotating = true;

            while (dist > 0.02f)
            {
                offsetPos = Vector3.SmoothDamp(offsetPos, targerOffsetPos, ref vel, smoothSpeed);
                dist = Vector3.Distance(offsetPos, targerOffsetPos);
                yield return null;
            }

            smoothRotating = false;
            offsetPos = targerOffsetPos;
        }
    }
}
