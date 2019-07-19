using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimStateTrackerFadeOut : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController.pc.animState = PlayerController.AnimState.FadeOut;
    }
}
