using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloqueoState : CombatState
{
    public BloqueoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        if (combatController.stats.EstaminaActual <= 0) return;

        combatController.anim.SetBool("running", false);
        combatController.setBloqueando(true);
        combatController.anim.SetTrigger("Bloqueo");
    }

    public override void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            combatController.GetComponent<Collider>().enabled = true;
            stateMachine.ChangeState(new IdleState(stateMachine, combatController));
        }
    }

    public override void Exit()
    {
        combatController.setBloqueando(false);
    }
}
