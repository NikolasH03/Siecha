using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueFuerte3 : CombatState
{
    public AtaqueFuerte3(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Fuerte3");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}
