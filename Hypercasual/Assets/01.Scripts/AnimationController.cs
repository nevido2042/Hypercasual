using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 캐릭터 애니메이터 컨트롤러 및 상태 연결을 관리
    /// </summary>
    public class AnimationController : MonoBehaviour
    {
        Animator anim;
        EightDirectionMovement movement;
        PlayerMining mining;

        void Start()
        {
            anim = GetComponent<Animator>();
            movement = GetComponent<EightDirectionMovement>();
            mining = GetComponent<PlayerMining>();
        }

        void Update()
        {
            // 1. 이동 상태에 따른 Run 파라미터 업데이트
            if (movement != null)
            {
                float mag = movement.input.magnitude;
                anim.SetBool("Run", mag > 0.5f); // 입력 크기가 일정 이상일 때 달리기
            }

            // 2. 채광 가능 여부에 따른 IsMining 파라미터 업데이트 (상체 레이어용)
            if (mining != null)
            {
                anim.SetBool("IsMining", mining.isMining);
            }
        }
    }
}
