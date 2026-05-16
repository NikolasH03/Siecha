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
        combatController.InvulneravilidadJugador();
        if (ControladorCambiarPersonaje.instance.getEsMuisca()) 
        { 
            apuntado.InstanciarBala(apuntado.ObtenerPosicionObjetivo());
            combatController.Reproducir("disparar_arco");

        }

        else
        {
            apuntado.EsferaDeDano();
            combatController.Reproducir("disparo_arcabuz");
            CameraShakeManager.instance.ShakeExplosion();
        }
        
    }
    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
    }
}
