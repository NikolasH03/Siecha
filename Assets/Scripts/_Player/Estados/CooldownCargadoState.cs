using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
public class CooldownCargadoState : CombatState
{
    private float recoveryTime;
    private float timer;
    public CooldownCargadoState(CombatStateMachine fsm, ControladorCombate cc, float recoveryTime) : base(fsm, cc) 
    {
        this.recoveryTime = recoveryTime;
    }
    public override void Enter()
    {
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recoveryTime)
        {
            stateMachine.ChangeState(new VerificarTipoArmaState(stateMachine, combatController));
        }
    }
}
