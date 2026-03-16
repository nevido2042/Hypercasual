using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 테스트 및 실제 게임 플레이를 위해 죄수를 주기적으로 생성
    /// </summary>
    public class PrisonerSpawner : MonoBehaviour
    {
        [Header("References")]
        public GameObject uniformModelPrefab;
        public Avatar uniformAvatar;
        public RuntimeAnimatorController prisonerAnimatorController;
        public GameObject[] prisonerVisuals;
        public PrisonerQueueManager queueManager;
        public Transform spawnPoint;

        [Header("Settings")]
        public int minRequired = 1;
        public int maxRequired = 5;
        public float moveSpeed = 5f;
        public float spawnInterval = 5f;
        [SerializeField] private float exitOffsetRange = 1.5f;
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
            if (queueManager == null || prisonerVisuals == null || prisonerVisuals.Length == 0) return;

            // 1. 랜덤 비주얼 데이터 선택
            GameObject visualPrefab = prisonerVisuals[Random.Range(0, prisonerVisuals.Length)];
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            
            // 비주얼을 감싸는 루트 오브젝트 생성
            GameObject prisonerGO = new GameObject("Prisoner_Dynamic");
            prisonerGO.transform.position = pos;
            prisonerGO.transform.rotation = Quaternion.identity;

            // 2. Prisoner 컴포넌트 부착
            Prisoner prisoner = prisonerGO.AddComponent<Prisoner>();
            
            // 3. 컴포넌트 초기화 및 비주얼 설정 (모든 비주얼은 동일한 아바타를 공유)
            // uniformAvatar를 기본 아바타로 사용하거나, visualData에서 공통으로 정의된 것을 사용
            prisoner.Initialize(minRequired, maxRequired, moveSpeed, exitOffsetRange, uniformModelPrefab, prisonerAnimatorController, uniformAvatar);
            prisoner.SetVisuals(visualPrefab, uniformModelPrefab);

            // 4. 네임스페이스 및 태그 설정
            prisonerGO.layer = LayerMask.NameToLayer("Default");

            queueManager.AddToQueue(prisoner);
        }
    }
}
