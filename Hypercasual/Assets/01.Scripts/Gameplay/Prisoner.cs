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
        public int minRequired = 1;
        public int maxRequired = 5;
        public float moveSpeed = 5f;

        [Header("Animations")]
        public Animator animator;
        private static readonly int HashWalk = Animator.StringToHash("Walk");

        [HideInInspector] public int requiredHandcuffs;
        [HideInInspector] public int currentHandcuffs = 0;
        
        private Rigidbody rb;
        private CapsuleCollider col;
        private MoneyStackZone moneyZone;
        private Queue<Vector3> moveQueue = new Queue<Vector3>();
        private Vector3 targetPosition;
        private bool isMoving = false;
        private bool isLeaving = false;

        public bool IsSatisfied => currentHandcuffs >= requiredHandcuffs;
        public int RemainingCount => Mathf.Max(0, requiredHandcuffs - currentHandcuffs);

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            moneyZone = Object.FindFirstObjectByType<MoneyStackZone>();

            // 물리 설정 초기화
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            rb.useGravity = false; // Y축 고정이므로 중력 불필요

            if (animator == null) animator = GetComponentInChildren<Animator>();
            requiredHandcuffs = Random.Range(minRequired, maxRequired + 1);
        }

        void FixedUpdate()
        {
            if (isMoving)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = targetPosition - currentPos;
                diff.y = 0; // 평면 이동

                if (diff.magnitude < 0.2f) // 도달 판정 범위를 조금 넓게 잡음 (물리 엔진 특성)
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
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);
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
            SetAnimationWalk(0f);
        }

        private void SetAnimationWalk(float value)
        {
            if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
            {
                animator.SetFloat(HashWalk, value);
            }
        }

        public void MoveTo(Vector3 position)
        {
            if (isLeaving) return;

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

            SetAnimationWalk(1f);
        }

        public void ReceiveHandcuff(Transform handcuff)
        {
            if (handcuff == null || isLeaving || this == null) return;
            currentHandcuffs++;

            // 수갑을 죄수의 자식으로 넣고 점프 연출
            handcuff.SetParent(transform);
            
            // 기존 트윈이 있다면 정리 (안전성)
            handcuff.DOKill();

            handcuff.DOLocalJump(Vector3.up * 1.5f, 1f, 1, 0.3f)
                .SetTarget(this.gameObject)
                .OnComplete(() => {
                    // 콜백 시점에 죄수나 수갑이 파괴되었을 수 있으므로 엄격한 체크
                    if (this == null || gameObject == null || transform == null) return;
                    
                    if (handcuff != null && handcuff.gameObject != null) 
                    {
                        Destroy(handcuff.gameObject);
                    }

                    // 수령 완료 시 펀치 스케일 연출
                    transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1f)
                        .SetTarget(this.gameObject);
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
        }

        private void LeaveArea()
        {
            moveQueue.Clear();

            // 매니저로부터 퇴장 경로 웨이포인트 가져오기
            PrisonerQueueManager manager = Object.FindFirstObjectByType<PrisonerQueueManager>();
            if (manager != null && manager.exitWaypoints != null)
            {
                for (int i = 0; i < manager.exitWaypoints.Count; i++)
                {
                    Transform wp = manager.exitWaypoints[i];
                    if (wp == null) continue;

                    Vector3 pos = wp.position;
                    // 마지막 웨이포인트(감옥 안)라면 약간의 랜덤 오프셋 추가
                    if (i == manager.exitWaypoints.Count - 1)
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
        }
    }
}
