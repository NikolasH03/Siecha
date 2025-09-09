using System.Collections.Generic;
using UnityEngine;

public class IdleMeleeState : CombatState
{
    public IdleMeleeState(CombatStateMachine fsm, ControladorCombate combatController) : base(fsm, combatController)
    {

    }

    private bool comboDetectado = false;
    public override void Enter()
    {
        combatController.anim.Play("movimiento Basico");
        combatController.anim.SetFloat("Velx", 0);
        combatController.anim.SetFloat("Vely", 0);
        combatController.anim.SetBool("running", false);
        combatController.setAtacando(false);
        combatController.DesactivarVentanaCombo();
        combatController.DesactivarTodosLosTrails();
        combatController.DesactivarTodosLosCollider();
    }
    public override void HandleInput()
    {
        if (InputJugador.instance.moverse.sqrMagnitude > 0.5f)
        {

            stateMachine.ChangeState(new MoverseMeleeState(stateMachine, combatController));
            return;

        }

        if (InputJugador.instance.cambiarArmaDistancia)
        {
            combatController.CambiarArmaDistancia();
            stateMachine.ChangeState(new VerificarTipoArmaState(stateMachine, combatController));
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

        if (TryExecuteFinisher()) return;

        if (InputJugador.instance.AtaqueLigero && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        if (InputJugador.instance.atacarFuerte && !combatController.anim.GetBool("running"))
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

            default:
                break;
        }
    }
}

