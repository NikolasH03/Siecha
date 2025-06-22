using UnityEngine;

public class MuerteTemporalState : CombatState
{
    private float tiempoEspera = 3f;
    private float tiempoActual;
    public MuerteTemporalState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc) { }

    public override void Enter()
    {
        combatController.anim.SetBool("Muere", true);
        combatController.anim.Play("morir");

        combatController.GetComponent<Collider>().enabled = false;
        combatController.GetComponent<Rigidbody>().isKinematic = true;
        combatController.CambiarMovimientoCanMove(false);

        tiempoActual = 0f;

    }
    public override void Update()
    {
        tiempoActual += Time.deltaTime;

        if (tiempoActual >= tiempoEspera)
        {
            HUDJugador hud = combatController.GetComponent<HUDJugador>();
            if (hud != null)
            {
                hud.MostrarCanvasMuerte();
            }
        }
    }
}

