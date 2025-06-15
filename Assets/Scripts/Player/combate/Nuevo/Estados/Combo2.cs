using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo2 : CombatState
{
    public Combo2(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.anim.SetTrigger("Combo2");
        combatController.setAtacando(true);
    }

    public override void Exit()
    {
        combatController.setAtacando(false);
    }
}
