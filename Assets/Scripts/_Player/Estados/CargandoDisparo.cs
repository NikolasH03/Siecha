using System.Collections.Generic;
using UnityEngine;
public class CargandoDisparo : CombatState
{
    public CargandoDisparo(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.anim.SetTrigger("CargarDisparo");
    }
    public override void HandleInput()
    {
        if (InputJugador.instance.holdSuccess)
        {
            stateMachine.ChangeState(new DisparoCargadoExitoso(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.holdFail)
        {
            stateMachine.ChangeState(new DisparoCargadoFallido(stateMachine, combatController));
            return;
        }
    }
    public override void Exit()
    {

    }
}
