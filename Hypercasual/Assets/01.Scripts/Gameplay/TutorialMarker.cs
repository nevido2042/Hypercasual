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

        private Transform _targetTransform;
        private Vector3 _currentOffset;
        private float _floatingY; // 애니메이션으로 제어할 추가 높이값
        private Tween _floatTween;
        private TutorialManager _manager;

        // 마커 오프셋 설정 (기존 TutorialManager에 있던 값들을 여기로 이동하여 캡슐화)
        [SerializeField] private Vector3 defaultOffset = new Vector3(0, 2.5f, 0);

        private void Awake()
        {
            // 그림자 비활성화 (마커를 더 깔끔하게 보이게 함)
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }

            _manager = Object.FindFirstObjectByType<TutorialManager>();
        }

        private void Start()
        {
            if (_manager != null)
            {
                _manager.OnStepChanged += UpdateStepMarker;
            }
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnStepChanged -= UpdateStepMarker;
            }
        }

        private void UpdateStepMarker(TutorialStep step)
        {
            Transform target = null;
            Vector3 offset = defaultOffset;

            switch (step)
            {
                case TutorialStep.Mining:
                    target = _manager.MiningArea;
                    RockGridGenerator grid = target != null ? target.GetComponent<RockGridGenerator>() : null;
                    if (grid != null) offset += grid.CenterOffset;
                    break;
                case TutorialStep.GemstoneDelivery:
                    target = _manager.GemstoneDeliveryZone;
                    break;
                case TutorialStep.HandcuffsStack:
                    target = _manager.HandcuffsStackZone;
                    break;
                case TutorialStep.HandcuffsDelivery:
                    target = _manager.HandcuffsDeliveryZone;
                    break;
                case TutorialStep.MoneyStack:
                    target = _manager.MoneyStackZone;
                    break;
                case TutorialStep.Complete:
                    Hide();
                    return;
            }

            SetTarget(target, offset);
        }

        private void OnEnable()
        {
            StartAnimation();
        }

        private void StartAnimation()
        {
            // 기존 트윈 제거 (가이드라인 준수)
            _floatTween?.Kill();
            
            // 1. 위아래로 부유하는 애니메이션 (가상 변수 제어)
            // 직접 transform을 움직이지 않고 _floatingY 값을 제어하여 LateUpdate에서 합산함
            _floatingY = 0f;
            _floatTween = DOTween.To(() => _floatingY, x => _floatingY = x, floatDistance, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private void Update()
        {
            // 2. 끊임없이 회전하는 연출
            transform.Rotate(rotateAxis, rotateSpeed * Time.deltaTime, rotateSpace);
        }

        private void LateUpdate()
        {
            // 3. 타겟 실시간 추적 (월드 좌표 기준)
            if (_targetTransform != null)
            {
                // 타겟 위치 + 설정된 오프셋 + 부유 애니메이션 값
                Vector3 targetPos = _targetTransform.position + _currentOffset;
                targetPos.y += _floatingY;
                
                transform.position = targetPos;
            }
        }

        /// <summary>
        /// 새로운 타겟 위로 마커 이동 및 활성화 (자식으로 들어가지 않음)
        /// </summary>
        public void SetTarget(Transform target, Vector3 offset)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            _targetTransform = target;
            _currentOffset = offset;
            
            gameObject.SetActive(true);
            
            // 초기 위치 즉시 설정
            transform.position = _targetTransform.position + _currentOffset;
            
            // 위치 변경 시 애니메이션 재시작
            StartAnimation();
        }

        public void Hide()
        {
            _targetTransform = null;
            gameObject.SetActive(false);
        }
    }
}
