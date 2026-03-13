using UnityEngine;

namespace Hero
{
    /// <summary>
    /// Transform의 자식을 깊게 탐색하기 위한 확장 메서드
    /// </summary>
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                
                Transform result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
