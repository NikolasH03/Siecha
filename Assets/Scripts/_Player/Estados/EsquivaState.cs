using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsquivaState : CombatState
{
    public EsquivaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.SetBool("dashing", true);
        combatController.anim.SetTrigger("Esquiva");
        combatController.InvulneravilidadJugador();
        combatController.DesplazamientoDash(combatController.ultimoInputMovimiento);
    }

    public override void HandleInput()
    {
        if (InputJugador.instance.cambiarArmaMelee)
        {
            combatController.CambiarArmaMelee();
        }

        if (InputJugador.instance.cambiarArmaDistancia)
        {
            combatController.CambiarArmaDistancia();
        }
    }
    public override void Exit()
    {
        combatController.anim.SetBool("dashing", false);
    }
}
