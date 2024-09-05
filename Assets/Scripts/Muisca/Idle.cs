using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Player.instance.atacandoDebil)
        {
            Player.instance.anim.Play("Attack1");
            Player.instance.numero_golpesDebiles++;
        }
        if (Player.instance.atacandoFuerte)
        {
            Player.instance.anim.Play("HeavyAttack1");
            Player.instance.numero_golpesFuertes++;
        }
      
        if (Player.instance.anim.GetBool("blocking"))
        {
            Player.instance.anim.Play("bloqueo");
           
            
        } 
        if (Player.instance.isDashing)
        {
            Player.instance.anim.Play("dash");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.instance.atacando = false;
        Player.instance.atacandoDebil = false;
        Player.instance.atacandoFuerte = false;
        Player.instance.isDashing = false;

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
