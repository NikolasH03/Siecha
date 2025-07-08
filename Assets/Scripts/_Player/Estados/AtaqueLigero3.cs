using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueLigero3 : CombatState
{
    public AtaqueLigero3(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Ligero3");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}
