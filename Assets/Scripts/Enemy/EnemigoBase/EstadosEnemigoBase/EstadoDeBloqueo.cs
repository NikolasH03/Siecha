using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Guardia activa: el enemigo se detiene y bloquea mientras el jugador siga atacando
/// (con un tiempo máximo de seguridad). Al terminar vuelve a seguir o a patrullar.
/// </summary>
public class EstadoDeBloqueo : EstadoBase
{
    private const float TiempoMaximoBloqueo = 5f;

    private readonly NavMeshAgent agent;
    private readonly HealthComp   vidaEnemigo;

    private float tiempoEnBloqueo;

    public EstadoDeBloqueo(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo)
        : base(enemigo, animator)
    {
        this.agent       = agent;
        this.vidaEnemigo = vidaEnemigo;
    }

    public override void OnEnter()
    {
        vidaEnemigo.setBloqueado(true);
        animator.CrossFade(BlockHash, duracionTransicion);
        agent.isStopped = true;
        tiempoEnBloqueo = 0f;
    }

    public override void Update()
    {
        tiempoEnBloqueo += Time.deltaTime;

        if (!enemigo.JugadorEstaAtacando() || tiempoEnBloqueo >= TiempoMaximoBloqueo)
            TerminarBloqueo();
    }

    public override void OnExit()
    {
        agent.isStopped = false;
        vidaEnemigo.setBloqueado(false);
    }

    private void TerminarBloqueo()
    {
        vidaEnemigo.setBloqueado(false);
        agent.isStopped = false;

        enemigo.VolverAComportamientoBase();
    }
}