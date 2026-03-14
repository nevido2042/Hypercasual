using UnityEngine;
using DG.Tweening;

namespace Hero
{
    public class MovingProduct : MonoBehaviour
    {
        private Transform targetZone;
        private float moveSpeed;
        private System.Action<Transform> onArrival;

        public void Setup(Transform destination, float speed, System.Action<Transform> arrivalCallback)
        {
            targetZone = destination;
            moveSpeed = speed;
            onArrival = arrivalCallback;
            
            float distance = Vector3.Distance(transform.position, targetZone.position);
            float duration = distance / moveSpeed;

            transform.DOMove(targetZone.position, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    onArrival?.Invoke(transform);
                });
        }
    }
}
