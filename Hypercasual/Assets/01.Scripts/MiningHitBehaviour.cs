using UnityEngine;

namespace Hero
{
    /// <summary>
    /// 애니메이터 상태 내부에서 특정 시간에 코드를 실행하기 위한 동작
    /// </summary>
    public class MiningHitBehaviour : StateMachineBehaviour
    {
        private bool hasHit = false; // 현재 루프 주기에서 타격이 발생했는지 여부
        
        [Header("타격 판정 타이밍 (0=시작, 1=끝)")]
        [Range(0f, 1f)]
        public float hitTime = 0.5f;

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 애니메이션 0~1 사이의 정규화된 시간 계산
            float currentTime = stateInfo.normalizedTime % 1.0f;

            // 설정한 hitTime을 통과하면 타격 로직 실행 (단 한 번만)
            if (currentTime >= hitTime && !hasHit)
            {
                hasHit = true;
                PlayerMining mining = animator.GetComponent<PlayerMining>();
                if (mining != null)
                {
                    mining.PerformMiningHit(); // 실제 타격 판정 실행
                }
            }
            // 루프가 돌아 처음으로 가면 타격 상태 초기화
            else if (currentTime < hitTime && hasHit)
            {
                hasHit = false;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 상태를 빠져나갈 때 반드시 초기화
            hasHit = false;
        }
    }
}
