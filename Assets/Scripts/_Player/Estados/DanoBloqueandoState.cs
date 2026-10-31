using UnityEngine;

public class DanoBloqueandoState : CombatState
{
    private int DanoRecibido;
    public DanoBloqueandoState(CombatStateMachine fsm, ControladorCombate cc, int Dano) : base(fsm, cc)
    { 
        this.DanoRecibido = Dano;
    }

    public override void Enter()
    {

        combatController.InvulneravilidadJugador();
        combatController.stats.UsarEstamina(DanoRecibido);

        if (combatController.stats.EstaminaActual <= 0)
        {
            RumbleManager.Play(TipoRumble.GuardiaRota);
            stateMachine.ChangeState(new PerderGuardiaState(stateMachine, combatController));
            return;
        }
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.anim.SetTrigger("DanoBloqueando");
        combatController.Reproducir("bloqueo");
        CameraShakeManager.instance.ShakeBloqueo();
        RumbleManager.Play(TipoRumble.BloqueoImpacto);
        combatController.EmpezarRegeneracionEstamina();

    }
    public override void Update()
    {
        if (combatController.stats.EstaminaActual > 0)
        {
            if (!InputJugador.instance.bloquear)
            {
                combatController.TerminarInvulnerabilidad();
                stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
            }

        }
    }

}
