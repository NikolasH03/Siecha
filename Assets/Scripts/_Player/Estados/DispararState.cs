using UnityEngine;

public class DispararState : CombatState
{
    private ControladorApuntado apuntado;
    
    private const float TIMEOUT_SEGUNDOS = 2f;
    private float tiempoEnEstado = 0f;

    public DispararState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        tiempoEnEstado = 0f;
        
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());
        apuntado.SetEstaApuntando(true);

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

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;
        
        if (tiempoEnEstado >= TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning("DispararState: timeout. El Animation Event no llegó. Revisa el Animator.");
            combatController.TerminarEstadoDisparo();
        }
    }

    public override void Exit()
    {
        combatController.TerminarInvulnerabilidad();
        apuntado.SetEstaApuntando(false);
    }
}