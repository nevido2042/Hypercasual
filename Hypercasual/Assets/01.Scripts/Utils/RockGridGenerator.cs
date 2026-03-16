using UnityEngine;

namespace Hero
{
    public class RockGridGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private int columns = 8;
        [SerializeField] private int rows = 30;
        [SerializeField] private Vector2 spacing = new Vector2(2f, 2f);

        public Vector3 CenterOffset => new Vector3(((columns - 1) * spacing.x) * 0.5f, 0, 0);

        /// <summary>
        /// 특정 월드 좌표가 이 그리드 영역 내부에 있는지 판별
        /// </summary>
        public bool IsInsideGrid(Vector3 worldPos)
        {
            // 월드 좌표를 이 오브젝트의 로컬 좌표로 변환
            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            // 그리드 생성 범위 계산 (약간의 여유분 0.5f 추가)
            float minX = -0.5f;
            float maxX = (columns - 1) * spacing.x + 0.5f;
            float minZ = -0.5f;
            float maxZ = (rows - 1) * spacing.y + 0.5f;

            return localPos.x >= minX && localPos.x <= maxX &&
                   localPos.z >= minZ && localPos.z <= maxZ;
        }

        [ContextMenu("Generate Rock Grid")]
        public void GenerateGrid()
        {
            if (rockPrefab == null) return;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Vector3 localPos = new Vector3(x * spacing.x, 0, z * spacing.y);
#if UNITY_EDITOR
                    GameObject rock = UnityEditor.PrefabUtility.InstantiatePrefab(rockPrefab) as GameObject;
                    if (rock != null)
                    {
                        rock.transform.SetParent(this.transform);
                        rock.transform.localPosition = localPos;
                    }
#endif
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 채광 영역 가시화
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f); // 짙은 노란색 (반투명)
            
            float width = (columns - 1) * spacing.x;
            float depth = (rows - 1) * spacing.y;
            Vector3 center = new Vector3(width * 0.5f, 0.5f, depth * 0.5f);
            Vector3 size = new Vector3(width + 1f, 1f, depth + 1f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(center, size);
            Gizmos.matrix = oldMatrix;
        }
    }
}
