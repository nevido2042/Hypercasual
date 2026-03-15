using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 드릴의 타격부에 부착되어 바위와 접촉 시 자동으로 채광을 실행하는 스크립트
    /// </summary>
    public class DrillHead : MonoBehaviour
    {
        [Header("Drill Settings")]
        [SerializeField] private float rotateSpeed = 720f;
        [SerializeField] private Transform rotateVisual; // 회전할 시각적 오브젝트
        
        [SerializeField] private LayerMask targetLayer;  // 감지할 레이어
        
        private bool isMining = false;

        void Awake()
        {
            // 설정되지 않았다면 자식 중에서 'Top_Front'를 탐색
            if (rotateVisual == null)
            {
                rotateVisual = transform.Find("Top_Front");
            }
        }

        void Update()
        {
            if (isMining && rotateVisual != null)
            {
                // 드릴 회전 연출 (Z축 기준)
                rotateVisual.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            }
        }

        public void SetActiveMining(bool active)
        {
            isMining = active;
        }

        public void SetTargetLayer(LayerMask layer)
        {
            targetLayer = layer;
        }

        private void OnTriggerStay(Collider other)
        {
            // 드릴이 활성 상태일 때만 채광 시도
            if (!isMining) return;

            // 레이어 체크 (유니티 레이어 마스크 방식)
            if (((1 << other.gameObject.layer) & targetLayer) != 0)
            {
                MineableRock rock = other.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    // 드릴은 플레이어의 자식이므로 최상단 루트(플레이어)를 전달
                    rock.Mine(transform.root.gameObject);
                }
            }
        }
    }
}
