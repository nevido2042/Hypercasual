using UnityEngine;
using DG.Tweening;

namespace Hero
{
    public class OffScreenIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float distanceFromPlayer = 2.5f; 
        [SerializeField] private float heightOffset = 0.01f; 
        [SerializeField] private float smoothSpeed = 10f; 
        [SerializeField] private Vector3 arrowRotateOffset = new Vector3(90, 0, 90); 

        [Header("References")]
        [SerializeField] private GameObject arrowVisual; 
        
        [Header("Animation")]
        [SerializeField] private float animPunchAmount = 0.2f; 
        [SerializeField] private float animDuration = 0.8f;

        private Camera _mainCam;
        private TutorialManager _tutorialManager;
        private TutorialMarker _targetMarker;
        private Transform _playerTransform;
        private Renderer[] _renderers;
        private bool _isVisible = true;

        private void Start()
        {
            _mainCam = Camera.main;
            _tutorialManager = Object.FindFirstObjectByType<TutorialManager>();
            
            _playerTransform = transform.root;
            if (_playerTransform == transform) 
                _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (arrowVisual == null && transform.childCount > 0)
                arrowVisual = transform.GetChild(0).gameObject;

            _renderers = GetComponentsInChildren<Renderer>(true);
            SetVisualVisible(false);
        }

        private void SetVisualVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;
            
            foreach (var r in _renderers) r.enabled = visible;

            if (visible)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }

        private void StartAnimation()
        {
            if (arrowVisual == null) return;
            
            arrowVisual.transform.DOKill();
            arrowVisual.transform.localPosition = Vector3.zero;
            
            // 로컬 X축 방향으로 찌르는 애니메이션
            arrowVisual.transform.DOPunchPosition(new Vector3(animPunchAmount, 0, 0), animDuration, 2, 0.5f)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(arrowVisual);
        }

        private void StopAnimation()
        {
            if (arrowVisual == null) return;
            arrowVisual.transform.DOKill();
            arrowVisual.transform.localPosition = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_tutorialManager == null) _tutorialManager = Object.FindFirstObjectByType<TutorialManager>();

            if (_tutorialManager == null)
            {
                SetVisualVisible(false);
                return;
            }

            _targetMarker = _tutorialManager.Marker;

            if (_targetMarker == null || !_targetMarker.gameObject.activeInHierarchy)
            {
                SetVisualVisible(false);
                return;
            }

            Vector3 targetPos = _targetMarker.transform.position;
            Vector3 screenPos = _mainCam.WorldToViewportPoint(targetPos);

            bool isOffScreen = screenPos.z < 0 || screenPos.x < 0.05f || screenPos.x > 0.95f || screenPos.y < 0.05f || screenPos.y > 0.95f;

            if (isOffScreen)
            {
                SetVisualVisible(true);

                if (_playerTransform == null) return;

                Vector3 playerPos = _playerTransform.position;
                Vector3 direction = (targetPos - playerPos);
                direction.y = 0; 
                
                if (direction.sqrMagnitude > 0.001f)
                {
                    direction.Normalize();

                    Vector3 desiredWorldPos = playerPos + direction * distanceFromPlayer;
                    
                    // 레이캐스트로 실제 지면 높이 찾기
                    if (Physics.Raycast(desiredWorldPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, ~0, QueryTriggerInteraction.Ignore))
                    {
                        desiredWorldPos.y = hit.point.y + heightOffset;
                    }
                    else
                    {
                        // 지면을 못 찾으면 플레이어 발 위치(y축 하단)로 가정
                        desiredWorldPos.y = playerPos.y + heightOffset;
                    }

                    transform.position = Vector3.Lerp(transform.position, desiredWorldPos, Time.deltaTime * smoothSpeed);

                    Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot * Quaternion.Euler(arrowRotateOffset), Time.deltaTime * smoothSpeed);
                }
            }
            else
            {
                SetVisualVisible(false);
            }
        }
    }
}
