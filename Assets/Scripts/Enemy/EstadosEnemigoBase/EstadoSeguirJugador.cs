using UnityEngine;
using UnityEngine.AI;

public class EstadoSeguirJugador : EstadoBase
{
    readonly NavMeshAgent agent;
    private float velocidadBase;
    private float velocidadPersecucion;

    public EstadoSeguirJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, float velocidadPersecucion)
        : base(enemigo, animator)
    {
        this.agent = agent;
        this.velocidadBase = agent.speed;
        this.velocidadPersecucion = velocidadPersecucion;
    }

    public override void OnEnter()
    {
        animator.CrossFade(RunningHash, duracionTransicion);
        agent.isStopped = false;
        agent.speed = velocidadPersecucion;
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;

        if (jugador == null)
        {
            Debug.LogWarning($"[{enemigo.name}] Jugador null en EstadoSeguir, volviendo a patrullar.");
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        if (!enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        agent.SetDestination(jugador.position);

        // FIX: SePuedeAtacarAlJugador ahora requiere el rango como parámetro
        // para evitar tener el dato duplicado entre DetectarJugador y EnemyStats.
        if (enemigo.EstaAtacando() && enemigo.detectarJugador.SePuedeAtacarAlJugador(enemigo.RangoDeAtaque))
        {
            enemigo.CambiarAEstado<EstadoAtacarJugador>();
        }
    }

    public override void OnExit()
    {
        agent.speed = velocidadBase;
    }
}