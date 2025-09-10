public class VerificarTipoArmaState : CombatState
{
    public VerificarTipoArmaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
    }

    public override void Enter()
    {
        int armaEquipada = combatController.VerificarArmaEquipada();

        if (armaEquipada == 1) 
        {
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }
        else if (armaEquipada == 2) 
        {
            stateMachine.ChangeState(new IdleDistanciaState(stateMachine, combatController));
        }
        else
        {
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }
    }
}
