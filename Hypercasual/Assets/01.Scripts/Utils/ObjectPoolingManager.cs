using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 이펙트 및 UI 텍스트의 오브젝트 풀링을 담당하는 매니저
    /// </summary>
    public class ObjectPoolingManager : MonoBehaviour
    {
        public static ObjectPoolingManager Instance { get; private set; }

        // 프리팹별 풀 관리를 위한 딕셔너리
        private Dictionary<int, ObjectPool<GameObject>> poolDictionary = new Dictionary<int, ObjectPool<GameObject>>();
        private Dictionary<int, Vector3> originalScales = new Dictionary<int, Vector3>();
        private Dictionary<int, Transform> groupRoots = new Dictionary<int, Transform>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 풀에서 오브젝트를 가져와 활성화 (프리팹 기반)
        /// </summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();

            if (!poolDictionary.ContainsKey(key))
            {
                // 프리팹별 전용 컨테이너 생성 (매니저 바로 아래)
                GameObject groupObj = new GameObject($"Pool_{prefab.name}");
                groupObj.transform.SetParent(this.transform);
                groupRoots[key] = groupObj.transform;

                poolDictionary[key] = new ObjectPool<GameObject>(
                    createFunc: () => {
                        GameObject obj = Instantiate(prefab);
                        // 생성 시점에는 일단 전용 컨테이너 하위로
                        obj.transform.SetParent(groupRoots[key]);
                        return obj;
                    },
                    actionOnGet: (obj) => {
                        obj.SetActive(true);
                    },
                    actionOnRelease: (obj) => {
                        obj.SetActive(false);
                        // 반환 시 다시 전용 컨테이너 하위로 복귀 (하이어라키 정리)
                        if (groupRoots.ContainsKey(key))
                        {
                            obj.transform.SetParent(groupRoots[key]);
                        }
                        // 반환 시 잔여 트윈 정리 (안전성)
                        DG.Tweening.DOTween.Kill(obj);
                    },
                    actionOnDestroy: (obj) => {
                        DG.Tweening.DOTween.Kill(obj);
                        Destroy(obj);
                    },
                    collectionCheck: false,
                    defaultCapacity: 20,
                    maxSize: 100
                );
            }

            if (!originalScales.ContainsKey(key))
            {
                originalScales[key] = prefab.transform.localScale;
            }

            GameObject spawnedObj = poolDictionary[key].Get();
            
            // 파티클 시스템인 경우 자동 반환을 위해 Stop Action 설정
            ParticleSystem ps = spawnedObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.stopAction = ParticleSystemStopAction.Callback;
            }

            // 풀 상태 확인을 위한 변수 (필요 시 로그 추가 가능)
            var pool = poolDictionary[key];
            
            spawnedObj.transform.SetPositionAndRotation(position, rotation);
            
            // 부모 설정: 지정된 부모가 없으면 프리팹 전용 컨테이너를 부모로 함
            spawnedObj.transform.SetParent(parent != null ? parent : groupRoots[key]);
            spawnedObj.transform.localScale = originalScales[key]; // 원본 프리팹 스케일로 복원

            var returnToPoolComp = spawnedObj.GetComponent<ReturnToPool>();
            if (returnToPoolComp == null) returnToPoolComp = spawnedObj.AddComponent<ReturnToPool>();
            
            returnToPoolComp.Setup(poolDictionary[key], prefab.name);

            return spawnedObj;
        }

        // 수동으로 풀에 반환할 때 사용하는 편의 기능
        public void Release(GameObject obj)
        {
            var rtp = obj.GetComponent<ReturnToPool>();
            if (rtp != null) rtp.Release();
            else Destroy(obj); // 풀링 대상이 아니면 그냥 파괴
        }
    }

    public class ReturnToPool : MonoBehaviour
    {
        private ObjectPool<GameObject> pool;
        private string poolName;
        private bool isReleased = false;

        public void Setup(ObjectPool<GameObject> targetPool, string name)
        {
            pool = targetPool;
            poolName = name;
            isReleased = false;
        }

        public void Release()
        {
            if (isReleased || pool == null) return;
            isReleased = true;

            // 반환 전 트윈 정리
            DG.Tweening.DOTween.Kill(gameObject);
            
            pool.Release(gameObject);
        }

        void OnParticleSystemStopped()
        {
            Release();
        }
    }
}
