using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecargarState : CombatState
{
    private ControladorApuntado apuntado;
    private bool esperandoResultado;
    private bool fueInterrumpido = false;

    public RecargarState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        combatController.anim.SetTrigger("Recarga");

        if (!ControladorCambiarPersonaje.instance.getEsMuisca())
        {
            esperandoResultado = true;
            apuntado.IniciarMinijuegoRecarga(OnFinMinijuego);
        }

        fueInterrumpido = false;
    }

    public override void HandleInput()
    {
        //if (InputJugador.instance.esquivar && !combatController.anim.GetBool("dashing"))
        //{
        //    fueInterrumpido = true;
        //    stateMachine.ChangeState(new EsquivaState(stateMachine, combatController));
        //    return;
        //}

        //if (InputJugador.instance.cambiarArmaMelee)
        //{
        //    fueInterrumpido = true;
        //    combatController.CambiarArmaMelee();
        //    InputJugador.instance.CambiarInputMelee();
        //    stateMachine.ChangeState(new IdleMeleeState(stateMachine, combatController));
        //    return;
        //}
    }

    private void OnFinMinijuego(bool fuePerfecta)
    {
        esperandoResultado = false;
        if (fuePerfecta)
        {
            combatController.anim.speed = 1.5f;
            combatController.ActivarBufoDisparo();
        }
        else
        {
            combatController.anim.speed = 0.5f;
        }
    }
    public void MarcarComoInterrumpido()
    {
        fueInterrumpido = true;

        if (esperandoResultado && apuntado != null)
        {
            apuntado.CancelarMinijuegoRecarga();
            esperandoResultado = false;
        }
    }

    public override void Exit()
    {
        if (fueInterrumpido)
        {
            Debug.Log("Recarga interrumpida - cleanup completo");

            combatController.anim.speed = 1f;

            if (apuntado != null)
            {
                apuntado.TransicionarLayerPeso(1, 0f, 0.1f);
                apuntado.NoEstaApuntando();
                apuntado.SetEstaApuntando(false);

                if (esperandoResultado)
                {
                    apuntado.CancelarMinijuegoRecarga();
                }
            }

            combatController.CambiarCanMove(false);
        }
        else
        {
            Debug.Log("Recarga completada normalmente - sin cleanup visual");
        }

        esperandoResultado = false;
        fueInterrumpido = false;
    }
}
