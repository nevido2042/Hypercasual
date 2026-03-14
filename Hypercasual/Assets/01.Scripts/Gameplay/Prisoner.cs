using UnityEngine;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 죄수 AI: 대기열 이동, 수갑 요구량 관리, 애니메이션 처리
    /// </summary>
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
        
        private Vector3 targetPosition;
        private bool isMoving = false;
        private bool isLeaving = false;

        public bool IsSatisfied => currentHandcuffs >= requiredHandcuffs;
        public int RemainingCount => Mathf.Max(0, requiredHandcuffs - currentHandcuffs);

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            requiredHandcuffs = Random.Range(minRequired, maxRequired + 1);
        }

        void Update()
        {
            if (isMoving)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
                {
                    isMoving = false;
                    SetAnimationWalk(0f);
                }
            }
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
            if (isLeaving && !isMoving) return;

            targetPosition = position;
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
            if (handcuff == null || isLeaving) return;
            currentHandcuffs++;

            handcuff.SetParent(transform);
            handcuff.DOLocalJump(Vector3.up * 1.5f, 1f, 1, 0.3f)
                .SetTarget(this.gameObject)
                .OnComplete(() => {
                    if (this == null || transform == null) return;
                    if (handcuff != null) Destroy(handcuff.gameObject);
                    transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1f).SetTarget(this.gameObject);
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
            Invoke(nameof(LeaveArea), 1.0f);
        }

        private void LeaveArea()
        {
            MoveTo(transform.position + transform.forward * 10f);
            Destroy(gameObject, 3f);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this.gameObject);
        }
    }
}
