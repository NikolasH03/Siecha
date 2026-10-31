using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Persigue al jugador a velocidad de persecución. Pasa a atacar cuando el enemigo
/// tiene slot de ataque y está dentro del rango; a patrulla si pierde al jugador.
/// </summary>
public class EstadoSeguirJugador : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly float velocidadBase;
    private readonly float velocidadPersecucion;

    public EstadoSeguirJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, float velocidadPersecucion)
        : base(enemigo, animator)
    {
        this.agent                = agent;
        this.velocidadBase        = agent.speed;
        this.velocidadPersecucion = velocidadPersecucion;
    }

    public override void OnEnter()
    {
        animator.CrossFade(RunningHash, duracionTransicion);
        agent.isStopped = false;
        agent.speed     = velocidadPersecucion;
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;

        if (jugador == null || !enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        agent.SetDestination(jugador.position);

        if (enemigo.EstaAtacando() && enemigo.detectarJugador.SePuedeAtacarAlJugador(enemigo.RangoDeAtaque))
            enemigo.CambiarAEstado<EstadoAtacarJugador>();
    }

    public override void OnExit() => agent.speed = velocidadBase;
}