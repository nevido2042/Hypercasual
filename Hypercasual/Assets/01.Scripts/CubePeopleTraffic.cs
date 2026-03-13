using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CubePeopleTraffic : MonoBehaviour
{
        NavMeshAgent agent;
        public Vector2 minmaxSpeed = new Vector2(0.5f, 1.5f);

        public int playerState = 0; //0=진입, 1=머무름
        public bool refreshDestination = false;
        bool dice;

        public float pauseTime = 1;
        float timeCount;

        // 웨이포인트(경유지)
        public int targetPoint;
        public GameObject destinationFolder;
        List<Transform> wayPoints = new List<Transform>();
        
        // 애니메이션
        Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            timeCount = pauseTime;

            if (destinationFolder != null)
            {
                int count = destinationFolder.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    wayPoints.Add(destinationFolder.transform.GetChild(i));
                }
            }
            else
            {
                print("DestinationFolder is empty, navmesh does not work. (Scene object " + transform.gameObject.name.ToString() + ").");
            }

            agent.speed = RandomSpeed();
            targetPoint = RandomPoint();
            refreshDestination = true;
        }


        void Update()
        {
            if (wayPoints.Count == 0)
            {
                return;
            }
            else
            {
                float dist = Vector3.Distance(wayPoints[targetPoint].position, transform.position);
                if (dist < 0.35f)
                {
                    // 도착함
                    if (!dice)
                    {
                        playerState = Random.Range(0, 2);
                        dice = true;
                    }

                    if (playerState == 1)
                    {
                        timeCount -= Time.deltaTime;    // 대기
                        if (timeCount < 0)
                        {
                            timeCount = pauseTime;
                            dice = false;
                            playerState = 0;    // 상태 초기화
                        }
                    }
                    else
                    {
                        if (dice) dice = false;
                        targetPoint = RandomPoint();    // 새로운 목표지점
                        refreshDestination = true;
                    }
                }

                if (refreshDestination)
                {
                    agent.SetDestination(wayPoints[targetPoint].position);
                    refreshDestination = false;
                }
            }
            anim.SetFloat("Walk", agent.velocity.magnitude);
        }

        public int RandomPoint()
        {
            int rPoint = -1;
            if (wayPoints.Count > 0)
            {
                rPoint = Random.Range(0, wayPoints.Count);
                
            }
            return rPoint;
        }

    public float RandomSpeed()
    {
        return Random.Range(minmaxSpeed.x, minmaxSpeed.y);
    }
}
