using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecargarState : CombatState
{
    private ControladorApuntado apuntado;
    private bool esperandoResultado;

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
    public override void Exit()
    {
        combatController.anim.speed = 1f;
    }
}
