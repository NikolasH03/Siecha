using System.Collections.Generic;
using UnityEngine;
public class DisparoCargadoExitoso : CombatState
{
    private ControladorApuntado apuntado;
    public DisparoCargadoExitoso(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) 
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }
    public override void Enter()
    {
        combatController.tipoAtaque = "cargado";
        combatController.anim.SetTrigger("DisparoExitoso");
        apuntado.InstanciarBala(apuntado.ObtenerPosicionObjetivo());
        Debug.Log("el disparo cargado fue exitoso");
    }

    public override void Exit()
    {

    }
}