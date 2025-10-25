using UnityEngine;

public class DanoState : CombatState
{
    private int DanoRecibido;
    public DanoState(CombatStateMachine fsm, ControladorCombate cc , int dano) : base(fsm, cc) 
    { 
        this.DanoRecibido = dano;
    }

    public override void Enter()
    {
        combatController.stats.RecibirDano(DanoRecibido);

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

            combatController.InvulneravilidadJugador();
            combatController.OrientarJugador();
            combatController.anim.SetTrigger("Dano");
            combatController.ReproducirVFX(5, 5);
            combatController.ReproducirSonido(5, 5);
            CameraShakeManager.instance.ShakeGolpeFuerte();

        }


    }

}

