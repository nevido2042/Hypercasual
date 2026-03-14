using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 머티리얼의 텍스처 오프셋을 조절하여 컨베이어 벨트가 움직이는 듯한 연출을 담당
    /// </summary>
    public class ConveyorBelt : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0, 1f); // 스크롤 속도 및 방향
        [SerializeField] private string texturePropertyName = "_BaseMap"; // URP는 _BaseMap, Standard는 _MainTex

        private Renderer beltRenderer;
        private Material beltMaterial;
        private Vector2 currentOffset;

        void Awake()
        {
            beltRenderer = GetComponent<Renderer>();
            if (beltRenderer != null)
            {
                beltMaterial = beltRenderer.material;
                
                // 셰이더 프로퍼티 존재 여부 확인 및 자동 설정 (편의성)
                if (!beltMaterial.HasProperty(texturePropertyName))
                {
                    if (beltMaterial.HasProperty("_BaseMap")) texturePropertyName = "_BaseMap";
                    else if (beltMaterial.HasProperty("_MainTex")) texturePropertyName = "_MainTex";
                }
            }
        }

        void Update()
        {
            if (beltMaterial == null) return;

            // 시간의 흐름에 따라 오프셋 계산 (오버플로우 방지를 위해 반복 처리)
            currentOffset += scrollSpeed * Time.deltaTime;
            currentOffset.x %= 1f;
            currentOffset.y %= 1f;

            // 머티리얼 오프셋 적용
            beltMaterial.SetTextureOffset(texturePropertyName, currentOffset);
        }

        private void OnDestroy()
        {
            // 인스턴스 생성된 머티리얼 정리 (메모리 누수 방지)
            if (beltMaterial != null)
            {
                Destroy(beltMaterial);
            }
        }
    }
}
