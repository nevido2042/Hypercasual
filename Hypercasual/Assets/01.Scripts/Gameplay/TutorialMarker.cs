using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 튜토리얼 중 특정 위치를 가리키며 부유 및 회전하는 마커
    /// </summary>
    public class TutorialMarker : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float floatDistance = 0.5f;
        [SerializeField] private float floatDuration = 1.0f;
        [SerializeField] private float rotateSpeed = 180f;
        [SerializeField] private Vector3 rotateAxis = Vector3.up;     // 회전축 설정
        [SerializeField] private Space rotateSpace = Space.World;    // 회전 공간 (World 또는 Self)

        private Vector3 startOffset;
        private Tween floatTween;

        private void Awake()
        {
            startOffset = transform.localPosition;

            // 그림자 비활성화 (마커를 더 깔끔하게 보이게 함)
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        private void OnEnable()
        {
            StartAnimation();
        }

        private void StartAnimation()
        {
            // 기존 트윈 제거 (가이드라인 준수)
            transform.DOKill();
            
            // 초기 위치 리셋
            transform.localPosition = startOffset;

            // 1. 위아래로 부유하는 애니메이션
            floatTween = transform.DOLocalMoveY(startOffset.y + floatDistance, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private void Update()
        {
            // 2. 끊임없이 회전하는 연출
            transform.Rotate(rotateAxis, rotateSpeed * Time.deltaTime, rotateSpace);
        }

        /// <summary>
        /// 새로운 타겟 위로 마커 이동 및 활성화
        /// </summary>
        public void SetTarget(Transform target, Vector3 offset)
        {
            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.SetParent(target);
            transform.localPosition = offset;
            startOffset = offset;
            
            // 위치 변경 시 애니메이션 재시작
            StartAnimation();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
