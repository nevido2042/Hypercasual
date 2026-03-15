using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

namespace Hero
{
    public class NavMeshBakeHelper : Editor
    {
        [MenuItem("Hero/Bake NavMesh")]
        public static void Bake()
        {
            Debug.Log("Starting NavMesh Bake...");
            
            // 바닥 오브젝트를 찾아 Static으로 설정
            GameObject floor = GameObject.Find("Plane");
            if (floor != null)
            {
                floor.isStatic = true;
                Debug.Log("Set 'Plane' to Static for baking.");
            }
            else
            {
                Debug.LogWarning("'Plane' object not found. Ensure the floor is named 'Plane' or set it to Static manually.");
            }

            NavMeshBuilder.BuildNavMesh();
            Debug.Log("NavMesh Bake Completed!");
        }
    }
}
