using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloqueoState : CombatState
{
    public BloqueoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        if (combatController.stats.EstaminaActual <= 0) return;

        combatController.setBloqueando(true);
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Bloqueo");
    }

    public override void HandleInput()
    {
        if (!InputJugador.instance.bloquear)
        {
            combatController.GetComponent<Collider>().enabled = true;
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }
    }

    public override void Update()
    {
        if (combatController.stats.EstaminaActual <= 0) return;
    }

    public override void Exit()
    {
        combatController.setBloqueando(false);
    }
}
