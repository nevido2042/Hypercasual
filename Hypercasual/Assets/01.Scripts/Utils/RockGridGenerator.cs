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
    }
}
