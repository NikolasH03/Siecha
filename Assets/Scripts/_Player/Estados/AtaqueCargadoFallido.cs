using System.Collections.Generic;
using UnityEngine;
public class AtaqueCargadoFallido : CombatState
{
    public AtaqueCargadoFallido(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("CargadoFallido");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
        combatController.TerminarInvulnerabilidad();
    }
}
