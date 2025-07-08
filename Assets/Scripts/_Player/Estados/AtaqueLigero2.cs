using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueLigero2 : CombatState
{
    public AtaqueLigero2(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    private bool comboDetectado = false;
    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Ligero2");
        combatController.setAtacando(true);
    }

    public override void HandleInput()
    {
        if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.bloquear)
        {
            combatController.DesactivarVentanaCombo();
            stateMachine.ChangeState(new BloqueoState(stateMachine, combatController));
            return;
        }

        if (!combatController.puedeHacerCombo) return;

        if (InputJugador.instance.atacarLigero)
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        else if (InputJugador.instance.atacarFuerte)
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
        if (comboDetectado) return;

        switch (combatController.inputBufferCombo)
        {
            case TipoInputCombate.Ligero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo = false;
                stateMachine.ChangeState(new AtaqueLigero3(stateMachine, combatController));
                break;

            case TipoInputCombate.Fuerte:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo = false;
                stateMachine.ChangeState(new AtaqueFuerte1(stateMachine, combatController));
                break;

            default:
                break;
        }
    }

    public override void Exit()
    {
        comboDetectado = false;
        combatController.setAtacando(false);
    }
}
