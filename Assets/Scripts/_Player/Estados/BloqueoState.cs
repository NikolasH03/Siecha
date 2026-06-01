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
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.anim.SetTrigger("Bloqueo");
    }

    public override void HandleInput()
    {
        if (combatController.stats.EstaminaActual <= 0) return;

        if (!InputJugador.instance.bloquear)
        {
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }
    }

    public override void Exit()
    {
        combatController.setBloqueando(false);
    }
}
