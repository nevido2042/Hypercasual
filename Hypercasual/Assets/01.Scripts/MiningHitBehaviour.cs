using UnityEngine;

public class MiningHitBehaviour : StateMachineBehaviour
{
    private bool hasHit = false;
    
    [Header("타격 판정 타이밍 (0=시작, 1=끝)")]
    [Range(0f, 1f)]
    public float hitTime = 0.5f;

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 0~1 사이의 진행도 
        float currentTime = stateInfo.normalizedTime % 1.0f;

        // 지정된 hitTime을 넘어가는 순간 1회 타격 판정 실행
        if (currentTime >= hitTime && !hasHit)
        {
            hasHit = true;
            EightDirectionMovement player = animator.GetComponent<EightDirectionMovement>();
            if (player != null)
            {
                player.PerformMiningHit();
            }
        }
        // 다음 루프 사이클로 넘어가면 타격 상태 초기화 (다시 때릴 수 있게 됨)
        else if (currentTime < hitTime && hasHit)
        {
            hasHit = false;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 채광 상태를 벗어나면 다음번 진입을 위해 초기화
        hasHit = false;
    }
}
