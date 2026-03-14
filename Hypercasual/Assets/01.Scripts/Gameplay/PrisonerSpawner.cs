using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 테스트 및 실제 게임 플레이를 위해 죄수를 주기적으로 생성
    /// </summary>
    public class PrisonerSpawner : MonoBehaviour
    {
        [Header("References")]
        public GameObject prisonerPrefab;
        public PrisonerQueueManager queueManager;
        public Transform spawnPoint;

        [Header("Settings")]
        public float spawnInterval = 5f;
        public bool autoSpawn = true;

        private float nextSpawnTime;

        void Update()
        {
            if (!autoSpawn) return;

            if (Time.time >= nextSpawnTime)
            {
                SpawnPrisoner();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        public void SpawnPrisoner()
        {
            if (prisonerPrefab == null || queueManager == null) return;

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject go = Instantiate(prisonerPrefab, pos, Quaternion.identity);
            
            Prisoner prisoner = go.GetComponent<Prisoner>();
            if (prisoner != null)
            {
                queueManager.AddToQueue(prisoner);
            }
        }
    }
}
