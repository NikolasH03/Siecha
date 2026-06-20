using UnityEngine;

public class RecargarState : CombatState
{
    private ControladorApuntado apuntado;
    private bool esperandoResultado;
    private bool fueInterrumpido = false;
    
    private const float TIMEOUT_SEGUNDOS = 5f;
    private float tiempoEnEstado = 0f;

    public RecargarState(CombatStateMachine fsm, ControladorCombate cc) : base(fsm, cc)
    {
        apuntado = cc.GetComponent<ControladorApuntado>();
    }

    public override void Enter()
    {
        tiempoEnEstado = 0f;
        fueInterrumpido = false;

        combatController.anim.SetTrigger("Recarga");
        
        if (!ControladorCambiarPersonaje.instance.getEsMuisca())
        {
            esperandoResultado = true;
            apuntado.IniciarMinijuegoRecarga(OnFinMinijuego);
        }
        combatController.CambiarCanMove(true);
    }

    public override void HandleInput()
    {
        // Espacio reservado para esquivar o cambiar arma durante recarga.
        // Si se implementa, marcar fueInterrumpido = true y llamar
        // apuntado.CancelarMinijuegoRecarga() si esperandoResultado.
    }

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;
        apuntado.EstaApuntando(apuntado.ObtenerPosicionObjetivo());

        if (tiempoEnEstado >= TIMEOUT_SEGUNDOS)
        {
            Debug.LogWarning("RecargarState: timeout. Revisa el Animation Event TerminarEstadoRecarga.");
            combatController.TerminarEstadoRecarga();
        }
    }

    private void OnFinMinijuego(bool fuePerfecta)
    {
        esperandoResultado = false;
        combatController.anim.speed = fuePerfecta ? 1.5f : 0.5f;
        if (fuePerfecta) combatController.ActivarBufoDisparo();
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
        combatController.anim.speed = 1f;

        if (esperandoResultado && apuntado != null)
            apuntado.CancelarMinijuegoRecarga();

        esperandoResultado = false;
        fueInterrumpido = false;
    }
}