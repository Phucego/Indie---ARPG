using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class OnFinish : StateMachineBehaviour
{
    [SerializeField] private string animation;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.GetComponent<PlayerMovement>().ChangeAnimation(animation, 0.2f, stateInfo.length);
        
    }
}
