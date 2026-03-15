using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 죄수 감지용 센서 클래스
    /// </summary>
    public class JailSensor : MonoBehaviour
    {
        public JailController controller;

        private void OnTriggerEnter(Collider other)
        {
            // 죄수 컴포넌트 확인
            if (other.TryGetComponent<Prisoner>(out var prisoner))
            {
                if (controller == null) return;

                controller.OnPrisonerEntered(other.gameObject);
            }
        }
    }
}
