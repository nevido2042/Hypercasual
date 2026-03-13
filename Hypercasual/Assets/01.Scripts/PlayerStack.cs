using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 플레이어 등 뒤에 젬스톤을 쌓고 휘청거리는 연출을 담당
    /// </summary>
    public class PlayerStack : MonoBehaviour
    {
        [Header("설정")]
        public Transform stackPoint;        // 젬스톤이 쌓이기 시작할 위치
        public float verticalSpacing = 0.2f; // 젬스톤 간의 수직 간격
        private List<Transform> stackList = new List<Transform>();
        private EightDirectionMovement movement;
        private Vector3 lastVelocity;
        private bool wasMoving = false;
        private Vector2 smoothInput;
        public float tiltSensitivity = 2f; // 이동 시 기울어지는 감도

        private Vector3 lastPosition;
        private Vector3 currentVelocity;
        private Vector2 smoothLean;

        void Start()
        {
            movement = GetComponent<EightDirectionMovement>();
            lastPosition = transform.position;
            
            if (stackPoint == null)
            {
                GameObject go = new GameObject("StackPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, 1f, -0.5f);
                stackPoint = go.transform;
            }
        }

        void Update()
        {
            HandlePhysicsSway();
        }

        /// <summary>
        /// 실제 위치 변화(가속도)를 바탕으로 관성 기울기 계산
        /// </summary>
        void HandlePhysicsSway()
        {
            if (stackList.Count == 0) return;

            // 1. 현재 속도 계산
            Vector3 worldVelocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            // 2. 캐릭터의 로컬 좌표계 기준 속도 (전진/후진, 좌/우)
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
            
            // 3. 속도 변화량(가속도)에 따른 기울기 목표값 설정
            // 가속할 때 뒤로(-X), 감속할 때 앞으로(+X), 우회전 시 왼쪽(+Z)
            // 속도 차이를 부드럽게 추적
            currentVelocity = Vector3.Lerp(currentVelocity, localVelocity, Time.deltaTime * 10f);
            Vector3 deltaVel = localVelocity - currentVelocity;

            float targetX = -deltaVel.z * tiltSensitivity;
            float targetZ = deltaVel.x * tiltSensitivity;

            // 4. 기울기 값을 부드럽게 보간 (찰랑거리는 느낌)
            smoothLean.x = Mathf.Lerp(smoothLean.x, targetX, Time.deltaTime * 5f);
            smoothLean.y = Mathf.Lerp(smoothLean.y, targetZ, Time.deltaTime * 5f);

            Quaternion targetRot = Quaternion.Euler(smoothLean.x, 0, smoothLean.y);

            // 5. 모든 체인 마디에 적용 (누적되어 탑이 휘어짐)
            foreach (var gem in stackList)
            {
                gem.localRotation = Quaternion.Slerp(gem.localRotation, targetRot, Time.deltaTime * 5f);
            }
        }

        public void AddToStack(GameObject gemstonePrefab)
        {
            Transform parent = stackList.Count == 0 ? stackPoint : stackList[stackList.Count - 1];
            Vector3 targetLocalPos = stackList.Count == 0 ? Vector3.zero : new Vector3(0, verticalSpacing, 0);

            GameObject gemGO = Instantiate(gemstonePrefab, stackPoint.position, Quaternion.identity);
            Gemstone gemstone = gemGO.GetComponent<Gemstone>();
            if (gemstone == null) gemstone = gemGO.AddComponent<Gemstone>();

            gemstone.AttachToStack(parent, targetLocalPos, () => {
                stackList.Add(gemstone.transform);
                gemstone.transform.localScale = Vector3.zero;
                gemstone.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            });
        }
    }
}
