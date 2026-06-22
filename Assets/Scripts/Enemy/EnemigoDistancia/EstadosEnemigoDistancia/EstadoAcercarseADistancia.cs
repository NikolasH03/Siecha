using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Versión del seguimiento para EnemigoDistancia.
/// 
/// DIFERENCIA CLAVE CON EstadoSeguirJugador:
/// EstadoSeguirJugador tiene hardcodeada una transición directa a
/// EstadoAtacarJugador cuando llega al rango. El arquero no tiene ese estado,
/// así que necesita su propia versión que simplemente se detenga al llegar
/// al rango y deje que EvaluarComportamiento decida qué hacer.
/// 
/// Modificar EstadoSeguirJugador para contemplar este caso introduciría
/// lógica condicional basada en el tipo de enemigo, que es exactamente
/// lo que la herencia existe para evitar.
/// </summary>
public class EstadoAcercarseADistancia : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly float        velocidadBase;
    private readonly float        velocidadPersecucion;
    private readonly float        rangoDeAtaque;

    public EstadoAcercarseADistancia(
        Enemigo      enemigo,
        Animator     animator,
        NavMeshAgent agent,
        float        velocidadPersecucion,
        float        rangoDeAtaque)
        : base(enemigo, animator)
    {
        this.agent                = agent;
        this.velocidadBase        = agent.speed;
        this.velocidadPersecucion = velocidadPersecucion;
        this.rangoDeAtaque        = rangoDeAtaque;
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

        // Al llegar al rango, detenerse y dejar que EvaluarComportamiento decida.
        // NO se transiciona directamente a ningún estado de ataque.
        // Al llegar al rango, detenerse y forzar la decisión.
        if (enemigo.detectarJugador.SePuedeAtacarAlJugador(rangoDeAtaque))
        {
            agent.isStopped = true;
            // Obligamos a la FSM a reevaluar en este mismo frame
            ((EnemigoDistancia)enemigo).EvaluarComportamiento();
        }
    }

    public override void OnExit()
    {
        agent.speed     = velocidadBase;
        agent.isStopped = false;
    }
}
