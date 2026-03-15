using UnityEngine;

namespace Hero
{
    public class CheatManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject cashPrefab;
        public GameObject gemstonePrefab;

        [Header("Settings")]
        public int amountPerPress = 10;

        private PlayerStack playerStack;

        private void Update()
        {
            if (playerStack == null)
            {
                playerStack = Object.FindFirstObjectByType<PlayerStack>();
            }

            if (playerStack == null) return;

            if (Input.GetKeyDown(KeyCode.M))
            {
                for (int i = 0; i < amountPerPress; i++)
                {
                    if (cashPrefab != null)
                    {
                        GameObject cashObj = Instantiate(cashPrefab);
                        playerStack.AddToMoneyStack(cashObj.transform);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                for (int i = 0; i < amountPerPress; i++)
                {
                    if (gemstonePrefab != null)
                    {
                        GameObject gemObj = Instantiate(gemstonePrefab);
                        playerStack.AddToStack(gemObj);
                    }
                }
            }
        }
    }
}
