using System.Collections.Generic;
using UnityEngine;
public class AtaqueLigero1 : CombatState
{
    public AtaqueLigero1(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    private bool comboDetectado = false;
    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Ligero1");
        combatController.setAtacando(true);

    }
    public override void HandleInput()
    {
        if (TryExecuteFinisher()) return;

        if (InputJugador.instance.esquivar)
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

        if (InputJugador.instance.AtaqueLigero)
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }

        if (InputJugador.instance.atacarFuerte)
        {
            combatController.inputBufferCombo = TipoInputCombate.Fuerte;
        }

        if (InputJugador.instance.holdStart)
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
        if (comboDetectado) return;

        switch (combatController.inputBufferCombo)
        {
            case TipoInputCombate.Ligero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo=false;
                stateMachine.ChangeState(new AtaqueLigero2(stateMachine, combatController));
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
        combatController.DesactivarTodosLosTrails();
        combatController.DesactivarTodosLosCollider();
    }
}

