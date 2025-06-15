using UnityEngine;

public class DanoState : CombatState
{
    public DanoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.stats.RecibirDano(combatController.stats.DanoBase);

        //if (combatController.stats.EstaMuerto)
        //{
        //    stateMachine.ChangeState(new MuerteTemporalState(stateMachine, combatController));
        //}
        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;
        combatController.CambiarMovimientoCanMove(false);
        combatController.anim.SetBool("running", false);
        combatController.anim.SetTrigger("Dano");
        combatController.AnimationEvent_ReproducirVFX(5, 5);
        combatController.AnimationEvent_ReproducirSonido(5, 5);

    }

}

