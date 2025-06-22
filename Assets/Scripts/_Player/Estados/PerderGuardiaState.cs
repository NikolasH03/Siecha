using UnityEngine;

public class PerderGuardiaState : CombatState
{
    public PerderGuardiaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.CambiarMovimientoCanMove(false);
        combatController.anim.SetBool("running", false);
        combatController.anim.SetTrigger("Dano");
        Debug.Log("Le rompieron la guardia");

    }

}

