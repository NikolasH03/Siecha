using UnityEngine;

public class IdleDistanciaState : CombatState
{
    public IdleDistanciaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.Play("movimientoArmaDistancia");
    }

    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            combatController.CambiarArmaMelee();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            combatController.CambiarArmaDistancia();
        }

        if (combatController.VerificarArmaEquipada() == 1)
        {
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }
        if (InputJugador.instance.apuntar)
        {
            stateMachine.ChangeState(new ApuntarState(stateMachine, combatController));
        }
        else if (Input.GetKeyDown(KeyCode.Q) && !combatController.anim.GetBool("dashing"))
        {
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }


    }

    public override void Exit()
    {
    }
}

