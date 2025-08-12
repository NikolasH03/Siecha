using System.Collections.Generic;
using UnityEngine;
public class AtaqueCorriendoFuerte : CombatState
{
    public AtaqueCorriendoFuerte(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("AtaqueCorriendoFuerte");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}

