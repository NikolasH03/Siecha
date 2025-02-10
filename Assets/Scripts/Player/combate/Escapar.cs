using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escapar : StateMachineBehaviour
{
    
    private ControladorCombate player;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = animator.GetComponent<ControladorCombate>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player.anim.GetBool("blocking"))
        {
            player.setAtacando(false);
            player.anim.Play("bloqueando");

        }

        if (player.anim.GetBool("dashing"))
        {
            player.setAtacando(false);
            player.anim.Play("dash");
            player.GetComponent<Collider>().enabled = false;
            player.GetComponent<Rigidbody>().isKinematic = true;

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
