using UnityEngine;

public abstract class CombatState
{
    protected CombatStateMachine stateMachine;
    protected ControladorCombate combatController;

    public CombatState(CombatStateMachine stateMachine, ControladorCombate combatController)
    {
        this.stateMachine = stateMachine;
        this.combatController = combatController;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void HandleInput() { }


    // Método común para verificar finisher
    protected bool TryExecuteFinisher()
    {
        var enemigo = combatController.DetectarEnemigoStunned(5f);
        if (enemigo != null && InputJugador.instance.FinisherInput)
        {
            Debug.Log("Ejecutando finisher desde: " + this.GetType().Name);
            stateMachine.ChangeState(new FinisherState(stateMachine, combatController, enemigo));
            return true;
        }
        return false;
    }
}
