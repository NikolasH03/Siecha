using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoverseMeleeState : CombatState
{
    private Animator anim;
    private ControladorMovimiento movimiento;
    private bool comboDetectado = false;

    public MoverseMeleeState(CombatStateMachine fsm, ControladorCombate combatController) : base(fsm, combatController)
    {
        movimiento = combatController.GetComponent<ControladorMovimiento>();
        anim = combatController.anim;
    }

    public override void Enter()
    {

        combatController.anim.Play("movimiento Basico");

        anim.SetBool("running", false);
        movimiento.setCanMove(true);

    }

    public override void HandleInput()
    {
        // en caso de volver a idle
        if (InputJugador.instance.moverse.sqrMagnitude < 0.01f)
        {
            stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        }

        // otros inputs que puede hacer
        if (InputJugador.instance.cambiarArmaDistancia)
        {
            combatController.CambiarArmaDistancia();
            InputJugador.instance.CambiarInputDistancia();
            stateMachine.ChangeState(new MoverseDistanciaState(stateMachine, combatController));
        }

        if (InputJugador.instance.cambiarProtagonista)
        {
            ControladorCambiarPersonaje.instance.CambiarProtagonista();
        }
        if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.bloquear)
        {
            if (combatController.stats.EstaminaActual <= 0) return;

            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new BloqueoState(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.correr && InputJugador.instance.moverse.sqrMagnitude > 0.01f)
        {
            anim.SetBool("running", true);
            movimiento.CambiarVelocidad(true);
        }
        if (!InputJugador.instance.correr || InputJugador.instance.moverse.sqrMagnitude < 0.01f)
        {
            anim.SetBool("running", false);
            movimiento.CambiarVelocidad(false);
        }

        if (TryExecuteFinisher()) return;

        if (InputJugador.instance.AtaqueLigero && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        else if (InputJugador.instance.AtaqueLigero && combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.CorriendoLigero;
        }

        if (InputJugador.instance.atacarFuerte && combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.CorriendoFuerte;
        }

        else if (InputJugador.instance.atacarFuerte && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Fuerte;
        }

        if (InputJugador.instance.holdStart && !combatController.anim.GetBool("running"))
        {
            stateMachine.ChangeState(new CargandoAtaque(stateMachine, combatController));
        }
        if (combatController.inputBufferCombo != TipoInputCombate.Ninguno)
        {
            combatController.secuenciaInputs.Add(combatController.inputBufferCombo);
        }
    }
    public override void Update()
    {
        if (combatController.VerificarArmaEquipada() == 2) return;

        if (comboDetectado) return;

        switch (combatController.inputBufferCombo)
        {
            case TipoInputCombate.Ligero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                stateMachine.ChangeState(new AtaqueLigero1(stateMachine, combatController));
                break;

            case TipoInputCombate.Fuerte:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                stateMachine.ChangeState(new AtaqueFuerte1(stateMachine, combatController));
                break;

            case TipoInputCombate.CorriendoLigero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                stateMachine.ChangeState(new AtaqueCorriendoLigero(stateMachine, combatController));
                break;
            case TipoInputCombate.CorriendoFuerte:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                stateMachine.ChangeState(new AtaqueCorriendoFuerte(stateMachine, combatController));
                break;

            default:
                break;
        }
    }

    public override void Exit()
    {
        anim.SetBool("running", false);
        movimiento.setCanMove(false);
    }
}

