using System.Collections.Generic;
using UnityEngine;

public class MoverseMeleeState : CombatState
{
    private Rigidbody rb;
    private Animator anim;
    private ControladorMovimiento movimiento;
    private bool comboDetectado = false;

    public MoverseMeleeState(CombatStateMachine fsm, ControladorCombate combatController) : base(fsm, combatController)
    {
        movimiento = combatController.GetComponent<ControladorMovimiento>();
        rb = combatController.GetComponent<Rigidbody>();
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
        if (InputJugador.instance.cambiarProtagonista)
        {
            ControladorCambiarPersonaje.instance.CambiarProtagonista();
        }

        if (InputJugador.instance.cambiarArmaDistancia)
        {
            combatController.CambiarArmaDistancia();
        }
        if (combatController.VerificarArmaEquipada() == 2)
        {
            InputJugador.instance.CambiarInputDistancia();
            stateMachine.ChangeState(new IdleDistanciaState(stateMachine, combatController));
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

        if (InputJugador.instance.atacarLigero && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        else if (InputJugador.instance.atacarFuerte && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Fuerte;
        }

        if (combatController.inputBufferCombo != TipoInputCombate.Ninguno)
        {
            combatController.secuenciaInputs.Add(combatController.inputBufferCombo);
            VerificarCombo();
        }
    }
    private void VerificarCombo()
    {
        if (combatController.secuenciaInputs.Count < 3) return;

        foreach (var combo in combatController.combos.Values)
        {
            if (SecuenciaCoincide(combo.secuencia, combatController.secuenciaInputs))
            {
                comboDetectado = true;
                stateMachine.ChangeState(combo.crearEstado(stateMachine, combatController));
                combatController.LimpiarSecuenciaInputs();
                return;
            }
        }
    }
    private bool SecuenciaCoincide(List<TipoInputCombate> a, List<TipoInputCombate> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }

        return true;
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

    public override void Exit()
    {
        anim.SetBool("running", false);
        movimiento.setCanMove(false);
    }
}

