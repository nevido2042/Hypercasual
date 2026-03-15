using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 죄수 AI: 대기열 이동, 수갑 요구량 관리, 애니메이션 처리
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Prisoner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int minRequired = 1;
        [SerializeField] private int maxRequired = 5;
        [SerializeField] private float moveSpeed = 5f;

        [Header("Animations")]
        [SerializeField] private Transform visualContainer;
        private Animator animator; // 루트 대신 활성화된 모델의 애니메이터를 가리킴
        private static readonly int HashWalk = Animator.StringToHash("Run");

        private RuntimeAnimatorController sharedController;
        private Avatar sharedAvatar;
        private GameObject uniformModelPrefab;

        public int RequiredHandcuffs => requiredHandcuffs; 
        private int requiredHandcuffs;
        public int CurrentHandcuffs => currentHandcuffs;
        private int currentHandcuffs = 0;
        
        private Rigidbody rb;
        private CapsuleCollider col;
        private MoneyStackZone moneyZone;
        private Queue<Vector3> moveQueue = new Queue<Vector3>();
        private Vector3 targetPosition;
        public bool IsMoving => isMoving;
        private bool isMoving = false;
        private bool isLeaving = false;
        private float jailEntryTimer = 0f;
        private const float JailEntryTimeout = 2f; // 감옥 진입 시 최대 대기 시간
        public bool HasEnteredJail { get; set; } = false;

        public bool IsSatisfied => currentHandcuffs >= requiredHandcuffs;
        public int RemainingCount => Mathf.Max(0, requiredHandcuffs - currentHandcuffs);

        void Awake()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            if (rb != null) return;

            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            
            col = GetComponent<CapsuleCollider>();
            if (col == null) col = gameObject.AddComponent<CapsuleCollider>();

            moneyZone = Object.FindFirstObjectByType<MoneyStackZone>();

            // 물리 설정 초기화
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.useGravity = false;

            // 콜라이더 기본 설정 (비주얼에 따라 조정될 수 있음)
            col.center = new Vector3(0, 0.5f, 0);
            col.radius = 0.25f;
            col.height = 1f;
        }

        public void Initialize(int minReq, int maxReq, float speed, GameObject uniformPrefab, RuntimeAnimatorController controller, Avatar avatar)
        {
            SetupComponents();

            this.minRequired = minReq;
            this.maxRequired = maxReq;
            this.moveSpeed = speed;
            this.uniformModelPrefab = uniformPrefab;

            requiredHandcuffs = Random.Range(minRequired, maxRequired + 1);

            // 애니메이션 데이터 저장
            this.sharedController = controller;
            this.sharedAvatar = avatar;

            // 비주얼 컨테이너 자동 생성
            if (visualContainer == null)
            {
                GameObject vc = new GameObject("VisualContainer");
                vc.transform.SetParent(transform);
                vc.transform.localPosition = Vector3.zero;
                vc.transform.localRotation = Quaternion.identity;
                visualContainer = vc.transform;
            }
        }

        public void SetVisuals(GameObject initialModelPrefab, GameObject uniformPrefab)
        {
            this.uniformModelPrefab = uniformPrefab;

            // 기존 비주얼 제거
            foreach (Transform child in visualContainer)
            {
                Destroy(child.gameObject);
            }

            // 초기 모델 생성
            if (initialModelPrefab != null)
            {
                GameObject model = Instantiate(initialModelPrefab, visualContainer);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                
                // 모델의 애니메이터 설정
                SetupModelAnimator(model);
            }
        }

        private void SetupModelAnimator(GameObject model)
        {
            animator = model.GetComponentInChildren<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();

            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.runtimeAnimatorController = sharedController;
                animator.avatar = sharedAvatar;
                
                // 현재 이동 상태 반영
                animator.SetBool(HashWalk, isMoving);
            }
        }

        private void SwapToUniformModel()
        {
            if (uniformModelPrefab == null) return;

            // 기존 비주얼 제거
            foreach (Transform child in visualContainer)
            {
                Destroy(child.gameObject);
            }

            // 유니폼 모델 생성
            GameObject model = Instantiate(uniformModelPrefab, visualContainer);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // 새 모델의 애니메이터 설정
            SetupModelAnimator(model);
        }

        private const float StoppingDistance = 0.25f;

        void FixedUpdate()
        {
            if (isMoving)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = targetPosition - currentPos;
                diff.y = 0; // 평면 이동

                if (diff.magnitude < StoppingDistance) 
                {
                    if (moveQueue.Count > 0)
                    {
                        SetNextDestination();
                    }
                    else
                    {
                        StopMoving();
                    }
                }
                else
                {
                    // 물리 기반 속도 설정
                    rb.velocity = diff.normalized * moveSpeed;
                    
                    // 회전 처리
                    Quaternion targetRot = Quaternion.LookRotation(diff.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 15f);

                    // 마지막 감옥 위치로 이동 중일 때 타임아웃 체크
                    if (isLeaving && moveQueue.Count == 0)
                    {
                        jailEntryTimer += Time.fixedDeltaTime;
                        if (jailEntryTimer >= JailEntryTimeout)
                        {
                            StopMoving();
                        }
                    }
                }
            }
            else
            {
                // 멈춰있을 때도 외부 힘에 의해 밀릴 수 있도록 속도 0 설정 유지
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            }
        }

        private void StopMoving()
        {
            isMoving = false;
            rb.velocity = Vector3.zero;
            SetAnimationWalk(false);

            // 감옥 안에 들어왔을 때만 뒤쪽(back)을 바라보게 회전
            if (HasEnteredJail)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back);
            }
            
            jailEntryTimer = 0f;
        }

        private void SetAnimationWalk(bool value)
        {
            if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
            {
                animator.SetBool(HashWalk, value);
            }
        }

        public void MoveTo(Vector3 position)
        {
            if (isLeaving) return;

            // 이미 동일한 위치로 이동 중이거나, 이미 목표 범위 내부에 있다면 무시
            if (isMoving && Vector3.Distance(targetPosition, position) < 0.1f) return;
            if (!isMoving && Vector3.Distance(transform.position, position) < StoppingDistance) return;

            moveQueue.Clear();
            moveQueue.Enqueue(position);
            
            if (!isMoving) SetNextDestination();
        }

        private void SetNextDestination()
        {
            if (moveQueue.Count == 0) return;

            targetPosition = moveQueue.Dequeue();
            isMoving = true;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            SetAnimationWalk(true);
        }

        public void ReceiveHandcuff(Transform handcuff)
        {
            if (handcuff == null || isLeaving || this == null) return;
            currentHandcuffs++;

            // 수갑을 죄수의 자식으로 넣고 점프 연출
            handcuff.SetParent(transform);
            
            handcuff.DOKill();
            handcuff.DOLocalJump(Vector3.up * 1.5f, 1f, 1, 0.3f)
                .SetLink(handcuff.gameObject)
                .OnComplete(() => {
                    // 콜백 시점에 죄수나 수갑이 파괴되었을 수 있으므로 엄격한 체크
                    if (this == null || gameObject == null || transform == null) return;
                    
                    if (handcuff != null && handcuff.gameObject != null) 
                    {
                        ObjectPoolingManager.Instance.Release(handcuff.gameObject);
                    }

                    // 수령 완료 시 펀치 스케일 연출
                    transform.DOKill();
                    transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1f)
                        .SetLink(this.gameObject);
                });

            if (IsSatisfied)
            {
                OnSatisfied();
            }
        }

        private void OnSatisfied()
        {
            if (isLeaving) return;
            isLeaving = true;

            // 현금 보상 생성
            if (moneyZone != null)
            {
                moneyZone.SpawnCash(requiredHandcuffs, transform.position);
            }

            LeaveArea();
            SwapToUniformModel();

            // 감옥 문을 열기 위해 JailController에 등록
            JailController jail = Object.FindFirstObjectByType<JailController>();
            if (jail != null)
            {
                jail.RegisterLeavingPrisoner(this);
            }
        }

        private void LeaveArea()
        {
            moveQueue.Clear();

            // 매니저로부터 퇴장 경로 웨이포인트 가져오기
            PrisonerQueueManager manager = Object.FindFirstObjectByType<PrisonerQueueManager>();
            if (manager != null && manager.ExitWaypoints != null)
            {
                for (int i = 0; i < manager.ExitWaypoints.Count; i++)
                {
                    Transform wp = manager.ExitWaypoints[i];
                    if (wp == null) continue;

                    Vector3 pos = wp.position;
                    // 마지막 웨이포인트(감옥 내부)라면 랜덤 오프셋 추가
                    if (i == manager.ExitWaypoints.Count - 1)
                    {
                        pos += new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                    }
                    moveQueue.Enqueue(pos);
                }
            }

            // 웨이포인트가 없으면 그냥 앞쪽으로 이동
            if (moveQueue.Count == 0)
            {
                moveQueue.Enqueue(transform.position + transform.forward * 10f);
            }

            SetNextDestination();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this.gameObject);

            // 파괴될 때 JailController에서 등록 해제 (예: 풀로 돌아가거나 할 때)
            JailController jail = Object.FindFirstObjectByType<JailController>();
            if (jail != null)
            {
                jail.UnregisterLeavingPrisoner(this);
            }
        }
    }
}
