using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 배달 구역의 젬스톤을 소모하여 수갑(Handcuffs)을 생산하는 기계 컨트롤러
    /// </summary>
    public class MachineController : MonoBehaviour
    {
        [Header("References")]
        public GemstoneDeliveryZone inputZone;   // 젬스톤을 가져올 구역
        public HandcuffsStackZone outputZone;   // 제품을 보낼 구역
        public GameObject productPrefab;         // 생산할 제품 프리팹 (수갑)
        public Transform inputPoint;              // 젬스톤이 머신으로 들어가는 입구
        public Transform outputPoint;             // 제품이 튀어나오는 출구

        [Header("Production Settings")]
        public float productionTime = 1.5f;       // 생산 소요 시간
        public float checkInterval = 0.5f;        // 젬스톤 유무 확인 간격

        private bool isWorking = false;

        void Start()
        {
            if (inputZone == null)
            {
                Debug.LogWarning($"{gameObject.name}: Input Zone이 설정되지 않았습니다.");
                return;
            }
            StartCoroutine(ProductionRoutine());
        }

        private IEnumerator ProductionRoutine()
        {
            while (true)
            {
                // 기계가 가동 중이 아니고 입력 구역에 젬스톤이 있다면 생산 시작
                if (!isWorking && inputZone.HasGem())
                {
                    yield return StartCoroutine(ProcessGem());
                }
                else
                {
                    yield return new WaitForSeconds(checkInterval);
                }
            }
        }

        private IEnumerator ProcessGem()
        {
            isWorking = true;

            // 1. 젬스톤 소모
            Transform gem = inputZone.ConsumeGem();
            if (gem != null)
            {
                // 젬스톤이 입구로 빨려 들어가는 연출
                gem.SetParent(transform);
                gem.DOJump(inputPoint.position, 1f, 1, 0.4f).SetEase(Ease.InQuad);
                gem.DOScale(Vector3.zero, 0.4f).OnComplete(() => {
                    // 실제 젬스톤 오브젝트는 풀로 반환되거나 파괴 (여기서는 파괴 처리 또는 비활성화)
                    gem.gameObject.SetActive(false);
                    // TODO: EffectManager 등을 통해 풀로 돌려보내는 로직 필요 (현재는 일단 비활성화)
                });

                yield return new WaitForSeconds(productionTime);

                // 2. 수갑 생산
                if (productPrefab != null)
                {
                    GameObject product = Instantiate(productPrefab, outputPoint.position, Quaternion.identity);
                    
                    // 생산된 제품이 튀어나오는 연출
                    product.transform.localScale = Vector3.zero;
                    product.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    
                    // 제품 출구 방향으로 살짝 밀어내거나 컨베이어 타게 함
                    if (outputZone != null)
                    {
                        outputZone.RegisterProduct(product);
                    }
                }
            }

            isWorking = false;
        }
    }
}
