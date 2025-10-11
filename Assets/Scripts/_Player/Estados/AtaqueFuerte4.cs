using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueFuerte4 : CombatState
{
    public AtaqueFuerte4(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador();
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("Fuerte4");
        combatController.setAtacando(true);
    }
    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }
}
