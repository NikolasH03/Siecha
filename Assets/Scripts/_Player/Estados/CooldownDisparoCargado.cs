using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
public class CooldownDisparoCargado : CombatState
{
    private float recoveryTime;
    private float timer;
    public CooldownDisparoCargado(CombatStateMachine fsm, ControladorCombate cc, float recoveryTime) : base(fsm, cc)
    {
        this.recoveryTime = recoveryTime;
    }
    public override void Enter()
    {
        timer = 0f;
        Debug.Log("entra al cooldown");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recoveryTime)
        {
            stateMachine.ChangeState(new RecargarState(stateMachine, combatController));
        }
    }
}
