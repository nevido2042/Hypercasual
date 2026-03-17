using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 수갑을 생산 구역에서 소모 구역으로 옮기거나, 소모 구역에서 죄수에게 배달하는 AI
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CrewAI : MonoBehaviour, IHandcuffProvider
    {
        private enum AIState { Idle, MovingToStack, MovingToDeliveryZone }

        [Header("References")]
        [SerializeField] private HandcuffsStackZone stackZone;
        [SerializeField] private HandcuffsConsumeZone consumeZone;
        [SerializeField] private HandcuffsDeliveryZone deliveryZone;
        [SerializeField] private Transform carryPoint;
        [SerializeField] private Transform frontStackPoint;

        [Header("Settings")]
        [SerializeField] private float arrivalDistance = 0.5f;
        [SerializeField] private float verticalSpacing = 0.1f;
        [SerializeField] private int maxCapacity = 5;
        [SerializeField] private string moveAnimParam = "Run";

        private NavMeshAgent agent;
        private Animator anim;
        private AIState currentState = AIState.Idle;
        private List<Transform> hcStack = new List<Transform>();
        private bool isInDeliveryZone = false;
        private bool isInStackZone = false;

        private void SetState(AIState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();

            if (agent != null)
            {
                agent.angularSpeed = 2000f; // 매우 빠른 회전
                agent.acceleration = 120f;  // 가속도도 높여서 즉각 반응하게 함
            }

            if (frontStackPoint == null)
            {
                GameObject go = new GameObject("FrontStackPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, 1f, 0.45f); // Same as player
                frontStackPoint = go.transform;
            }
        }

        private void Start()
        {
            if (stackZone == null) stackZone = FindFirstObjectByType<HandcuffsStackZone>();
            if (consumeZone == null) consumeZone = FindFirstObjectByType<HandcuffsConsumeZone>();
            if (deliveryZone == null) deliveryZone = FindFirstObjectByType<HandcuffsDeliveryZone>();

            StartCoroutine(BehaviorSequence());
        }

        private void Update()
        {
            if (anim != null)
            {
                float currentSpeed = agent.velocity.magnitude;
                anim.SetFloat(moveAnimParam, currentSpeed);

                // 이동 방향으로 즉시 회전 (NavMeshAgent의 기본 회전보다 더 빠르게 반응하도록 보정)
                if (currentSpeed > 0.1f && agent.velocity.sqrMagnitude > 0.01f)
                {
                    Vector3 lookDir = agent.velocity.normalized;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
            }
        }

        private IEnumerator BehaviorSequence()
        {
            WaitForSeconds waitHalf = new WaitForSeconds(0.5f);
            WaitForSeconds waitSmall = new WaitForSeconds(0.2f);

            while (true)
            {
                // 1. 수갑을 들고 있을 때: 무조건 배달 구역으로 이동
                if (hcStack.Count > 0)
                {
                    yield return StartCoroutine(ActionMoveToDeliveryZone());
                }
                // 2. 비어있을 때: 생산존에 제품이 있고 소모존에 수갑이 부족하면 보충하러 감 (높은 우선순위)
                else if (stackZone != null && stackZone.HasProducts && consumeZone != null && !consumeZone.HasHandcuffs())
                {
                    yield return StartCoroutine(ActionTakeFromStack());
                }
                // 3. 비어있지만 소모존에 수갑이 있고 죄수가 대기 중이면: 배달 구역 활성화를 위해 이동
                else if (consumeZone != null && consumeZone.HasHandcuffs() && consumeZone.IsPrisonerWaiting())
                {
                    yield return StartCoroutine(ActionMoveToDeliveryZone());
                }
                else
                {
                    SetState(AIState.Idle);
                    yield return waitHalf;
                }
                yield return waitSmall;
            }
        }

        private bool NeedsToDeliver()
        {
            return consumeZone != null && consumeZone.IsPrisonerWaiting();
        }

        private IEnumerator ActionTakeFromStack()
        {
            if (stackZone == null) yield break;
            SetState(AIState.MovingToStack);
            agent.SetDestination(stackZone.transform.position);
            
            // 트리거가 먼저 닿거나 거리가 가까워지면 진입
            while (!isInStackZone && Vector3.Distance(transform.position, stackZone.transform.position) > arrivalDistance)
            {
                if (stackZone == null || !stackZone.HasProducts) yield break;
                yield return new WaitForSeconds(0.1f);
            }
            
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // 물리적 관성 제거 및 즉각 정지

            while (stackZone.HasProducts && hcStack.Count < maxCapacity)
            {
                Transform item = stackZone.TakeProduct();
                if (item != null) AddToFrontStack(item);
                yield return new WaitForSeconds(0.1f);
            }
            agent.isStopped = false;
        }

        private IEnumerator ActionMoveToDeliveryZone()
        {
            if (deliveryZone == null) yield break;
            SetState(AIState.MovingToDeliveryZone);
            
            // 이미 정지 상태라면(이미 구역 안에서 대기 중이었다면) 불필요하게 풀지 않음
            if (!isInDeliveryZone)
            {
                agent.isStopped = false;
                agent.SetDestination(deliveryZone.transform.position);
            }
            
            while (deliveryZone != null)
            {
                if (!isInDeliveryZone)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    // 딜리버리 존에 도착함. 
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero; // 물리적 관성 제거
                    
                    yield return new WaitUntil(() => 
                        (hcStack.Count == 0 && (stackZone.HasProducts || consumeZone == null || !consumeZone.HasHandcuffs() || !consumeZone.IsPrisonerWaiting())) || 
                        !isInDeliveryZone);
                    
                    break;
                }

                // 이동 중에 상황이 변하면 이탈
                bool shouldExitEarly = hcStack.Count == 0 && (consumeZone == null || !consumeZone.HasHandcuffs() || !consumeZone.IsPrisonerWaiting());
                bool needRefill = hcStack.Count == 0 && stackZone != null && stackZone.HasProducts;
                
                if (shouldExitEarly || needRefill)
                {
                    break;
                }
            }
            
            agent.isStopped = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<HandcuffsDeliveryZone>() != null)
            {
                isInDeliveryZone = true;
            }
            else if (other.GetComponent<HandcuffsStackZone>() != null)
            {
                isInStackZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<HandcuffsDeliveryZone>() != null)
            {
                isInDeliveryZone = false;
            }
            else if (other.GetComponent<HandcuffsStackZone>() != null)
            {
                isInStackZone = false;
            }
        }


        public void AddToFrontStack(Transform item)
        {
            Transform parent = hcStack.Count == 0 ? frontStackPoint : hcStack[hcStack.Count - 1];
            Vector3 targetLocalPos = hcStack.Count == 0 ? Vector3.zero : new Vector3(0, verticalSpacing, 0);

            item.SetParent(parent);
            item.DOKill();
            item.DOLocalJump(targetLocalPos, 1.5f, 1, 0.25f)
                .SetEase(Ease.OutQuad)
                .SetLink(item.gameObject);
            item.DOLocalRotate(Vector3.zero, 0.25f)
                .SetLink(item.gameObject);
            
            item.localScale = Vector3.zero;
            item.DOScale(1f, 0.2f)
                .SetEase(Ease.OutBack)
                .SetLink(item.gameObject);

            hcStack.Add(item);
        }

        public Transform RemoveFromFrontStack()
        {
            if (hcStack.Count == 0) return null;
            Transform item = hcStack[hcStack.Count - 1];
            hcStack.RemoveAt(hcStack.Count - 1);
            return item;
        }

        public bool HasHandcuffs() => hcStack.Count > 0;
    }
}
