using UnityEngine;

public class DanoBloqueandoState : CombatState
{
    public DanoBloqueandoState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {

        combatController.InvulneravilidadJugador();
        combatController.stats.UsarEstamina(combatController.stats.DanoBase);

        if (combatController.stats.EstaminaActual <= 0)
        {
            stateMachine.ChangeState(new PerderGuardiaState(stateMachine, combatController));
        }
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("DanoBloqueando");
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
