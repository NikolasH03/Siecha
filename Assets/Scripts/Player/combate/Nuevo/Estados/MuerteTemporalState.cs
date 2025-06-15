using UnityEngine;

public class MuerteTemporalState : CombatState
{
    public MuerteTemporalState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.SetBool("Muere", true);
        combatController.anim.Play("morir");

        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;

        //combatController.jugadorMuerto = true;
    }
}

