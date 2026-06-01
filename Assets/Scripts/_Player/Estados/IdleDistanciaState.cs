using Unity.VisualScripting;
using UnityEngine;

public class IdleDistanciaState : CombatState
{

    public IdleDistanciaState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.Play("movimientoArmaDistancia");
        combatController.anim.SetFloat("Velx", 0);
        combatController.anim.SetFloat("Vely", 0);
        combatController.ResetCompleto();
    }

    public override void HandleInput()
    {
        if (InputJugador.instance.moverse.sqrMagnitude > 0.01f)
        {

            stateMachine.ChangeState(new MoverseDistanciaState(stateMachine, combatController));
            return;
        }

        if (InputJugador.instance.cambiarArmaMelee)
        {
            combatController.CambiarArmaMelee();
            InputJugador.instance.CambiarInputMelee();
            stateMachine.ChangeState(new VerificarTipoArmaState(stateMachine, combatController));
        }

        //if (InputJugador.instance.cambiarProtagonista)
        //{
        //    ControladorCambiarPersonaje.instance.CambiarProtagonista();
        //}
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

