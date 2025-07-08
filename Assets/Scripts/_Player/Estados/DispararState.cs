using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispararState : CombatState
{
    private ControladorApuntado apuntado;

    public DispararState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)  
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        combatController.anim.SetTrigger("Disparo");

        if (ControladorCambiarPersonaje.instance.getEsMuisca())
        {
            apuntado.InstanciarBala(apuntado.ObtenerPosicionObjetivo());
        }
        else
        {
            apuntado.EsferaDeDano();
            combatController.ReproducirVFX(4, 2);
            combatController.ReproducirSonido(4, 2);
            CameraShakeManager.instance.ShakeExplosion();
        }
        
    }
}
