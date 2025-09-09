using UnityEngine;

public class MoverseDistanciaState : CombatState
{
    private Animator anim;
    private ControladorMovimiento movimiento;

    public MoverseDistanciaState(CombatStateMachine fsm, ControladorCombate combatController) : base(fsm, combatController)
    {
        movimiento = combatController.GetComponent<ControladorMovimiento>();
        anim = combatController.anim;
    }

    public override void Enter()
    {
        combatController.anim.Play("movimientoArmaDistancia");

        anim.SetBool("running", false);
        movimiento.setCanMove(true);
    }

    public override void HandleInput()
    {
        // en caso de volver a idle
        if (InputJugador.instance.moverse.sqrMagnitude < 0.01f)
        {
            stateMachine.ChangeState(new IdleDistanciaState(stateMachine, combatController));
        }

        // otros inputs que puede hacer
        if (InputJugador.instance.cambiarArmaMelee)
        {
            combatController.CambiarArmaMelee();
            InputJugador.instance.CambiarInputMelee();
            stateMachine.ChangeState(new MoverseMeleeState(stateMachine, combatController));
        }

        if (InputJugador.instance.cambiarProtagonista)
        {
            ControladorCambiarPersonaje.instance.CambiarProtagonista();
        }

        if (InputJugador.instance.apuntar)
        {
            stateMachine.ChangeState(new ApuntarState(stateMachine, combatController));
        }
        else if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        {
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }
    }

    public override void Exit()
    {
        anim.SetBool("running", false);
        movimiento.setCanMove(false);
    }
}

