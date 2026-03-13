using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 채굴된 젬스톤의 개별 동작 (비행 및 상태 관리)
    /// </summary>
    public class Gemstone : MonoBehaviour
    {
        private Collider col;
        private Rigidbody rb;

        void Awake()
        {
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// 젬스톤을 플레이어 등 뒤의 로컬 목표 위치에 즉시 적재
        /// </summary>
        public void AttachToStack(Transform parent, Vector3 localTargetPos, System.Action onComplete)
        {
            // 물리 및 충돌 비활성화
            if (col != null) col.enabled = false;
            if (rb != null) rb.isKinematic = true;

            // 즉시 부모 설정 및 위치 고정
            transform.SetParent(parent);
            transform.localPosition = localTargetPos;
            transform.localRotation = Quaternion.identity;
            
            onComplete?.Invoke();
        }
    }
}
