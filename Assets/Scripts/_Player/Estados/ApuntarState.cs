using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApuntarState : CombatState
{
    private ControladorApuntado apuntado;
    private bool saltarTransicionLayer = false;
    public ApuntarState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        combatController.anim.SetTrigger("Apuntar");
        apuntado.TransicionarLayerPeso(1, 1f, 0.2f);
 
    }

    public override void HandleInput()
    {
        if (InputJugador.instance.disparar)
        {
            saltarTransicionLayer=true;
            stateMachine.ChangeState(new DispararState(stateMachine, combatController));
            return;
        }

        if (!InputJugador.instance.apuntar)
        {           
            stateMachine.ChangeState(new IdleDistanciaState(stateMachine, combatController));
            return;
        }
        if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        {            
            stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
            return;
        }
    }

    public override void Update()
    {      
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());
    }
    public override void Exit()
    {
        if (!saltarTransicionLayer) 
        {
            apuntado.TransicionarLayerPeso(1, 0f, 0.2f);
            apuntado.NoEstaApuntando();
        }
        
        saltarTransicionLayer = false;
        apuntado.SetEstaApuntando(false);
        
    }
}
