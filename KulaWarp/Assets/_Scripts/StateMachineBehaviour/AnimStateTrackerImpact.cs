using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimStateTrackerImpact : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController.pc.animState = PlayerController.AnimState.Impact;
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        PlayerController.pc.resetScale();
    }
}
