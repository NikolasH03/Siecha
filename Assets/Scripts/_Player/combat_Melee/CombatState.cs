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
}
