using UnityEngine;

public class ApuntarState : CombatState
{
    private ControladorApuntado apuntado;

    public ApuntarState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        if (!InputJugador.instance.apuntar) { return; }

        combatController.anim.SetTrigger("Apuntar");

        // ApuntarState es el punto de entrada al modo distancia.
        // Es responsable de abrir el layer. Los estados internos
        // (Disparar, Recargar, CargandoDisparo) no lo tocan.
        apuntado.TransicionarLayerPeso(1, 1f, 0.2f);
        combatController.CambiarCanMove(true);
    }

    public override void HandleInput()
    {
        // if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        // {
        //     stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
        //     return;
        // }

        if (InputJugador.instance.AtaqueLigero)
        {
            stateMachine.ChangeState(new DispararState(stateMachine, combatController));
            return;
        }

        if (InputJugador.instance.holdStart)
        {
            if (ControladorCambiarPersonaje.instance.getEsMuisca())
                stateMachine.ChangeState(new CargandoDisparo(stateMachine, combatController));
            else
                stateMachine.ChangeState(new DispararState(stateMachine, combatController));
            return;
        }

        if (!InputJugador.instance.apuntar)
        {
            stateMachine.ChangeState(new IdleDistanciaState(stateMachine, combatController));
            return;
        }
    }

    public override void Update()
    {
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());
    }

    public override void Exit()
    {
        apuntado.SetEstaApuntando(false);
        combatController.CambiarCanMove(false);
    }
}