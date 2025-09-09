using System.Collections.Generic;
using UnityEngine;
public class DisparoCargadoFallido : CombatState
{
    public DisparoCargadoFallido(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }
    public override void Enter()
    {
        combatController.tipoAtaque = "cargado";
        combatController.anim.SetTrigger("DisparoFallido");
        Debug.Log("el disparo cargado fue fallido");

    }

    public override void Exit()
    {
    }
}
