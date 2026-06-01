using System.Collections.Generic;
using UnityEngine;
public class CargandoDisparo : CombatState
{
    private ControladorApuntado apuntado;
    public CargandoDisparo(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>(); 
    }

    public override void Enter()
    {
        combatController.anim.SetTrigger("CargarDisparo");
        combatController.Reproducir("inicio_disparo_cargado");
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

    public override void Update()
    {
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());
    }

    public override void Exit()
    {

    }
}
