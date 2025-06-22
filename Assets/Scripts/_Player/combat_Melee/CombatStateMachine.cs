using UnityEngine;

public class CombatStateMachine : MonoBehaviour
{
    private CombatState currentState;
    public void ChangeState(CombatState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }

    public void Update()
    {
        currentState?.HandleInput();
        currentState?.Update();
    }
}
