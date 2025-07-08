using UnityEngine;

public class DanoState : CombatState
{
    public DanoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.stats.RecibirDano(combatController.stats.DanoBase);

        if (combatController.stats.VidaActual <= 0)
        {
            combatController.muertesActuales++;

            if (combatController.muertesActuales >= combatController.muertesMaximas)
                stateMachine.ChangeState(new MuerteDefinitivaState(stateMachine, combatController));
            
            else
                stateMachine.ChangeState(new MuerteTemporalState(stateMachine, combatController));
            
                
        }
        else
        {

            combatController.GetComponent<Collider>().enabled = false;
            combatController.GetComponent<Rigidbody>().isKinematic = true;
            combatController.CambiarMovimientoCanMove(false);
            combatController.anim.SetBool("running", false);
            combatController.OrientarJugador();
            combatController.anim.SetTrigger("Dano");
            combatController.ReproducirVFX(5, 5);
            combatController.ReproducirSonido(5, 5);
            CameraShakeManager.instance.ShakeGolpeFuerte();

        }


    }

}

