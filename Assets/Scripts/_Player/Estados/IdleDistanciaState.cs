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
        if (InputJugador.instance.cambiarArmaMelee)
        {
            combatController.CambiarArmaMelee();
        }

        if (combatController.VerificarArmaEquipada() == 1)
        {
            InputJugador.instance.CambiarInputMelee();
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
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

}

