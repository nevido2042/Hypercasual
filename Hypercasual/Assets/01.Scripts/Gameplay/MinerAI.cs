using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 자동으로 주변 바위를 찾아 채광하는 광부 AI
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MinerAI : MonoBehaviour
    {
        [Header("Mining Settings")]
        public float searchRadius = 20f;
        public float miningDistance = 1.5f;
        public LayerMask rockLayer;
        public GameObject miningTool;
        
        [Header("Animations")]
        public string miningAnimParam = "IsMining";
        public string moveSpeedParam = "Run";

        private NavMeshAgent agent;
        private Animator anim;
        private MineableRock targetRock;
        private bool isMining = false;
        private float nextMineTime = 0f;
        private float mineInterval = 1f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();

            // 툴이 설정되지 않은 경우 'Pickaxe' 이름을 가진 자식 탐색
            if (miningTool == null)
            {
                Transform foundTool = transform.FindDeepChild("Pickaxe");
                if (foundTool != null) miningTool = foundTool.gameObject;
            }

            // 곡괭이를 항상 켜둠
            if (miningTool != null) miningTool.SetActive(true);
        }

        private void Start()
        {
            StartCoroutine(BehaviorRoutine());
        }

        private void Update()
        {
            if (anim != null)
            {
                // 실제 이동 중일 때만 Run 애니메이션 활성화
                bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
                anim.SetBool(moveSpeedParam, isMoving);
            }

            if (isMining)
            {
                if (targetRock == null || !targetRock.CanBeMined)
                {
                    StopMining();
                    return;
                }

                if (Time.time >= nextMineTime)
                {
                    targetRock.Mine(gameObject);
                    nextMineTime = Time.time + mineInterval;
                }
            }
        }

        private IEnumerator BehaviorRoutine()
        {
            while (true)
            {
                if (targetRock == null || !targetRock.CanBeMined)
                {
                    FindTargetRock();
                }

                if (targetRock != null)
                {
                    float dist = Vector3.Distance(transform.position, targetRock.transform.position);
                    if (dist <= miningDistance)
                    {
                        if (!isMining) StartMining();
                    }
                    else
                    {
                        if (isMining) StopMining();
                        agent.SetDestination(targetRock.transform.position);
                        agent.isStopped = false;
                    }
                }
                else
                {
                    if (isMining) StopMining();
                    yield return new WaitForSeconds(1f);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void FindTargetRock()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, rockLayer);
            float minDistance = float.MaxValue;
            MineableRock closestRock = null;

            foreach (var col in colliders)
            {
                MineableRock rock = col.GetComponent<MineableRock>();
                if (rock != null && rock.CanBeMined)
                {
                    float dist = Vector3.Distance(transform.position, rock.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestRock = rock;
                    }
                }
            }
            targetRock = closestRock;
        }

        private void StartMining()
        {
            isMining = true;
            agent.isStopped = true;
            if (anim != null) anim.SetBool(miningAnimParam, true);
            if (miningTool != null) miningTool.SetActive(true);
            nextMineTime = Time.time + mineInterval;

            if (targetRock != null)
            {
                Vector3 lookPos = targetRock.transform.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }
        }

        private void StopMining()
        {
            isMining = false;
            agent.isStopped = false;
            if (anim != null) anim.SetBool(miningAnimParam, false);
            if (miningTool != null) miningTool.SetActive(false);
        }
    }
}
