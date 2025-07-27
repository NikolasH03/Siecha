using UnityEngine;

public class PerderGuardiaState : CombatState
{
    public PerderGuardiaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.CambiarMovimientoCanMove(false);
        combatController.anim.SetBool("running", false);
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Dano");
        combatController.ReproducirVFX(7, 5);
        Debug.Log("Le rompieron la guardia");

    }

}

