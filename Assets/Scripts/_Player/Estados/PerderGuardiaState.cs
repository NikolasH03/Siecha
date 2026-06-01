using UnityEngine;

public class PerderGuardiaState : CombatState
{
    public PerderGuardiaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("GuardBreak");
        combatController.Reproducir("perder_guardia");

    }
    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }

}

