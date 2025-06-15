using System.Collections.Generic;
using UnityEngine;

public class IdleState : CombatState
{
    public IdleState(CombatStateMachine fsm, ControladorCombate combatController) : base(fsm, combatController)
    {
    }

    private bool comboDetectado = false;
    public override void Enter()
    {
        combatController.anim.SetBool("running", false);
        combatController.setAtacando(false);
        combatController.anim.Play("movimiento Basico");
    }
    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !combatController.anim.GetBool("dashing")) 
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if (combatController.stats.EstaminaActual <= 0) return;

            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new BloqueoState(stateMachine, combatController));
            return;
        }

        if (Input.GetMouseButtonDown(0) && !combatController.anim.GetBool("running"))
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        else if (Input.GetMouseButtonDown(1) && !combatController.anim.GetBool("running"))
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
}

