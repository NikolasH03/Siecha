using System.Collections.Generic;
using UnityEngine;
public class AtaqueLigero4 : CombatState
{
    public AtaqueLigero4(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)     {  }

    public override void Enter()
    {
        combatController.tipoAtaque = "ligero";
        combatController.OrientarJugador();
        combatController.anim.SetTrigger("Ligero4");
        combatController.setAtacando(true);
    }
    public override void HandleInput()
    {
        if (combatController.statsBase.maxAtaquesLigeros <= 4) return;

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



        if (InputJugador.instance.atacarLigero)
        {
            combatController.inputBufferCombo = TipoInputCombate.Ligero;
        }
        else if (InputJugador.instance.atacarFuerte)
        {
            combatController.inputBufferCombo = TipoInputCombate.Fuerte;
        }
    }
    public override void Update()
    {
        if (combatController.statsBase.maxAtaquesLigeros <= 4) return;

        switch (combatController.inputBufferCombo)
        {
            case TipoInputCombate.Ligero:
                combatController.inputBufferCombo = TipoInputCombate.Ninguno;
                combatController.puedeHacerCombo = false;
                stateMachine.ChangeState(new AtaqueLigero5(stateMachine, combatController));
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
        combatController.setAtacando(false);
    }
}

