using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecargarState : CombatState
{
    private ControladorApuntado apuntado;

    public RecargarState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        combatController.anim.SetTrigger("Recarga");
    }
}
