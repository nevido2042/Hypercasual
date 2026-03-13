using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 군중 AI 무작위 이동 제어 스크립트
    /// </summary>
    public class CubePeopleTraffic : MonoBehaviour
    {
        NavMeshAgent agent;
        public Vector2 minmaxSpeed = new Vector2(0.5f, 1.5f); // AI 이동 속도 범위

        public int playerState = 0; // 0: 이동, 1: 대기 
        public bool refreshDestination = false;
        bool dice;

        public float pauseTime = 1; // 대기 시간
        float timeCount;

        public int targetPoint; // 목표 웨이포인트 인덱스
        public GameObject destinationFolder; // 웨이포인트 그룹 폴더
        List<Transform> wayPoints = new List<Transform>();
        
        Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            timeCount = pauseTime;

            // 목적지 폴더 내의 모든 자식 요소를 웨이포인트로 등록
            if (destinationFolder != null)
            {
                int count = destinationFolder.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    wayPoints.Add(destinationFolder.transform.GetChild(i));
                }
            }

            // 초기 속도 및 목적지 무작위 설정
            agent.speed = RandomSpeed();
            targetPoint = RandomPoint();
            refreshDestination = true;
        }

        void Update()
        {
            if (wayPoints.Count == 0) return;

            // 목적지 도달 확인
            float dist = Vector3.Distance(wayPoints[targetPoint].position, transform.position);
            if (dist < 0.35f)
            {
                // 상태 주사위 굴리기 (이동할지 대기할지)
                if (!dice)
                {
                    playerState = Random.Range(0, 2);
                    dice = true;
                }

                if (playerState == 1) // 대기 상태
                {
                    timeCount -= Time.deltaTime;    
                    if (timeCount < 0)
                    {
                        timeCount = pauseTime;
                        dice = false;
                        playerState = 0;    
                    }
                }
                else // 새로운 경로 탐색
                {
                    if (dice) dice = false;
                    targetPoint = RandomPoint();    
                    refreshDestination = true;
                }
            }

            // 목적지 갱신
            if (refreshDestination)
            {
                agent.SetDestination(wayPoints[targetPoint].position);
                refreshDestination = false;
            }

            // 보행 애니메이션 속도 비례 전환
            anim.SetFloat("Walk", agent.velocity.magnitude);
        }

        public int RandomPoint()
        {
            if (wayPoints.Count > 0) return Random.Range(0, wayPoints.Count);
            return -1;
        }

        public float RandomSpeed()
        {
            return Random.Range(minmaxSpeed.x, minmaxSpeed.y);
        }
    }
}
