using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo3 : CombatState
{
    public Combo3(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("Combo3");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
        combatController.TerminarInvulnerabilidad();
    }
}
