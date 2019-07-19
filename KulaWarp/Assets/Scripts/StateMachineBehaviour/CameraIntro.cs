using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIntro : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.applyRootMotion = false;
        CameraController.cc.camState = CameraController.CamState.Anim;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CameraController.cc.camState = CameraController.CamState.Default;
        animator.applyRootMotion = true;
    }
}
