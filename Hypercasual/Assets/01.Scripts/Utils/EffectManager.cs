using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 이펙트 및 UI 텍스트의 오브젝트 풀링을 담당하는 매니저
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

        // 프리팹별 풀 관리를 위한 딕셔너리
        private Dictionary<int, ObjectPool<GameObject>> poolDictionary = new Dictionary<int, ObjectPool<GameObject>>();

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
                // 새로운 풀 생성 (Unity 2021+ 내장 ObjectPool 사용)
                poolDictionary[key] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: (obj) => {
                        obj.SetActive(true);
                    },
                    actionOnRelease: (obj) => {
                        obj.SetActive(false);
                    },
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: false,
                    defaultCapacity: 10,
                    maxSize: 50
                );
            }

            GameObject spawnedObj = poolDictionary[key].Get();
            spawnedObj.transform.SetPositionAndRotation(position, rotation);
            spawnedObj.transform.SetParent(parent);

            // 풀로 되돌려주기 위한 정보 설정 (컴포넌트가 있다면)
            var returnToPoolComp = spawnedObj.GetComponent<ReturnToPool>();
            if (returnToPoolComp == null) returnToPoolComp = spawnedObj.AddComponent<ReturnToPool>();
            
            returnToPoolComp.Setup(poolDictionary[key]);

            return spawnedObj;
        }
    }

    /// <summary>
    /// 오브젝트를 다시 풀로 안전하게 돌려보내는 헬퍼 클래스
    /// </summary>
    public class ReturnToPool : MonoBehaviour
    {
        private ObjectPool<GameObject> pool;
        private bool isReleased = false;

        public void Setup(ObjectPool<GameObject> targetPool)
        {
            pool = targetPool;
            isReleased = false;
        }

        public void Release()
        {
            if (isReleased) return;
            isReleased = true;
            pool.Release(gameObject);
        }

        // 파티클 시스템이 있다면 재생 완료 시 자동 반환 처리
        void OnParticleSystemStopped()
        {
            Release();
        }
    }
}
