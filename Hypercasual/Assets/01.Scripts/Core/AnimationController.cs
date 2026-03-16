using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 캐릭터 애니메이터 컨트롤러 및 상태 연결을 관리
    /// </summary>
    public class AnimationController : MonoBehaviour
    {
        Animator anim;
        PlayerMovement movement;
        PlayerMining mining;

        void Start()
        {
            anim = GetComponent<Animator>();
            movement = GetComponent<PlayerMovement>();
            mining = GetComponent<PlayerMining>();
        }

        void Update()
        {
            // 1. 이동 상태에 따른 Run 파라미터 업데이트
            if (movement != null && mining != null)
            {
                float mag = movement.InputValue.magnitude;
                // 드릴카에 실제로 탑승 중일 때만 뛰지 않고 가만히 서있음(Idle)
                float speed = (mag > 0.5f && !mining.IsBoardingDrillCar) ? mag : 0f;
                anim.SetFloat("Run", speed);
            }

            // 2. 채광 가능 여부에 따른 IsMining 파라미터 업데이트 (상체 레이어용)
            // 드릴일 때는 채광 애니메이션(곡괭질)을 실행하지 않음
            if (mining != null)
            {
                bool shouldAnimate = mining.IsMining && !mining.IsDrillUpgraded;
                anim.SetBool("IsMining", shouldAnimate);
            }
        }
    }
}
