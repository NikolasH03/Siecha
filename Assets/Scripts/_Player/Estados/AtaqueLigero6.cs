using System.Collections.Generic;
using UnityEngine;
public class AtaqueLigero6 : CombatState
{
    public AtaqueLigero6(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Ligero6");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}

