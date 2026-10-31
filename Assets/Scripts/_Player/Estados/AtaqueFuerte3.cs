using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueFuerte3 : CombatState
{
    public AtaqueFuerte3(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.tipoAtaque = "fuerte";
        combatController.OrientarJugador(combatController.ultimoInputMovimiento);
        if (combatController.statsBase.maxAtaquesFuertes <= 3) combatController.InvulneravilidadJugador();
        combatController.anim.SetTrigger("Fuerte3");
        combatController.setAtacando(true);
        combatController.Reproducir("slash");
    }
    public override void HandleInput()
    {
        if (combatController.statsBase.maxAtaquesFuertes <= 3) return;

        if (IntentaEjecutarFinisher()) return;

        if (IntentaHacerAccionDefensiva()) return;

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
        if (combatController.statsBase.maxAtaquesFuertes <= 3) return;

        switch (combatController.inputBufferCombo)
        {
            case TipoInputCombate.Ligero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo = false;
                stateMachine.ChangeState(new AtaqueLigero1(stateMachine, combatController));
                break;

            case TipoInputCombate.Fuerte:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo = false;
                stateMachine.ChangeState(new AtaqueFuerte4(stateMachine, combatController));
                break;

            default:
                break;
        }
    }

    public override void Exit()
    {
        combatController.DesactivarTodosLosTrails();
        combatController.DesactivarTodosLosCollider();
        combatController.TerminarInvulnerabilidad();
    }
}
