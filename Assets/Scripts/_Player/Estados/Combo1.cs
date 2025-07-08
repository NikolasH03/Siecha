using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo1 : CombatState
{
    public Combo1(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Combo1");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}

