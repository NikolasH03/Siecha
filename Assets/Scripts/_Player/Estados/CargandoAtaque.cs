using System.Collections.Generic;
using UnityEngine;
public class CargandoAtaque : CombatState
{
    public CargandoAtaque(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("CargarAtaque");
        combatController.setAtacando(true);

    }
    public override void HandleInput()
    {
        if (InputJugador.instance.holdSuccess && !combatController.anim.GetBool("running"))
        {
            stateMachine.ChangeState(new AtaqueCargadoExitoso(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.holdFail && !combatController.anim.GetBool("running"))
        {
            stateMachine.ChangeState(new AtaqueCargadoFallido(stateMachine, combatController));
            return;
        }
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
        combatController.DesactivarTodosLosTrails();
    }
}
