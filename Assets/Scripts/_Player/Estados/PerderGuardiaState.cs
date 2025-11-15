using UnityEngine;

public class PerderGuardiaState : CombatState
{
    public PerderGuardiaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("GuardBreak");
        combatController.ReproducirVFX(7, 5);

    }
    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }

}

