using UnityEngine;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 죄수 대기열을 관리하고 위치를 배정
    /// </summary>
    public class PrisonerQueueManager : MonoBehaviour
    {
        [Header("Queue Settings")]
        [SerializeField] private Transform queueStartPoint;
        [SerializeField] private float spacing = 1.8f;
        [SerializeField] private int maxQueueSize = 5;

        public IReadOnlyList<Transform> ExitWaypoints => exitWaypoints;
        [Header("Path Settings")]
        [SerializeField] private List<Transform> exitWaypoints = new List<Transform>();

        private List<Prisoner> waitingPrisoners = new List<Prisoner>();

        public bool IsQueueEmpty => waitingPrisoners.Count == 0;

        public Prisoner GetFrontPrisoner()
        {
            if (waitingPrisoners.Count == 0) return null;
            return waitingPrisoners[0];
        }

        public void AddToQueue(Prisoner prisoner)
        {
            if (waitingPrisoners.Count >= maxQueueSize)
            {
                if (prisoner != null) Destroy(prisoner.gameObject);
                return;
            }

            waitingPrisoners.Add(prisoner);
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < waitingPrisoners.Count; i++)
            {
                if (waitingPrisoners[i] == null) continue;
                Vector3 targetPos = queueStartPoint.position - queueStartPoint.forward * (i * spacing);
                waitingPrisoners[i].MoveTo(targetPos);
            }
        }

        void Update()
        {
            bool changed = false;
            for (int i = waitingPrisoners.Count - 1; i >= 0; i--)
            {
                if (waitingPrisoners[i] == null || !waitingPrisoners[i].gameObject.activeInHierarchy || waitingPrisoners[i].IsSatisfied)
                {
                    waitingPrisoners.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                UpdatePositions();
            }
        }
    }
}
