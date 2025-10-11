using UnityEngine;

public class PerderGuardiaState : CombatState
{
    public PerderGuardiaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.OrientarJugador();
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("Dano");
        combatController.ReproducirVFX(7, 5);

    }
    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }

}

