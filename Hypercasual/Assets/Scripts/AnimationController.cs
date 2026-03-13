using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Animator anim;
    EightDirectionMovement movement;
    public bool run;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<EightDirectionMovement>();
        if (run) run = false;
    }

    void Update()
    {
        if (movement != null)
        {
            // 조이스틱이나 키보드 입력 크기가 0.5보다 크면 달리기 애니메이션 재생
            float mag = movement.input.magnitude;
            run = mag > 0.5f;
        }
        else
        {
            if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
            {
                run = false;
            }
            else
            {
                run = true;
            }
        }

        anim.SetBool("Run", run);
    }
}
