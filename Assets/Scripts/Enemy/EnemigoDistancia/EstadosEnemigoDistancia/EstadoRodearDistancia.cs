using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Versión del rodeo para EnemigoDistancia.
/// 
/// DIFERENCIA CON EstadoRodearJugador:
/// El base tiene hardcodeado: si EstaAtacando() → EstadoAtacarJugador.
/// El arquero no tiene EstadoAtacarJugador, así que cuando EnemyManager
/// le asigna un slot, este estado simplemente se detiene y deja que
/// EvaluarComportamiento decida el siguiente paso (apuntar o acercarse).
/// </summary>
public class EstadoRodearDistancia : EstadoBase
{
    private readonly NavMeshAgent agent;
    private Vector3 posicionObjetivo;
    private float   tiempoHastaActualizar;
    private const float INTERVALO_ACTUALIZACION = 2f;

    public EstadoRodearDistancia(Enemigo enemigo, Animator animator, NavMeshAgent agent)
        : base(enemigo, animator)
    {
        this.agent = agent;
    }

    public override void OnEnter()
    {
        animator.CrossFade(WalkingHash, duracionTransicion);
        ActualizarPosicion();
        tiempoHastaActualizar = INTERVALO_ACTUALIZACION;
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;

        if (enemigo.GetHealthComp().getBloqueando() || enemigo.GetHealthComp().EstaEsquivando)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (jugador == null || !enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        // Cuando EnemyManager asigna slot: detenerse y esperar EvaluarComportamiento.
        // NO transicionar directamente a EstadoAtacarJugador.
        // Cuando EnemyManager asigna slot: detenerse y evaluar
        if (enemigo.EstaAtacando())
        {
            agent.isStopped = true;
            // Pasamos a apuntar o acercarnos inmediatamente
            ((EnemigoDistancia)enemigo).EvaluarComportamiento(); 
            return;
        }

        tiempoHastaActualizar -= Time.deltaTime;
        if (tiempoHastaActualizar <= 0f)
        {
            ActualizarPosicion();
            tiempoHastaActualizar = INTERVALO_ACTUALIZACION;
        }

        agent.SetDestination(posicionObjetivo);

        Vector3 lookPos = jugador.position;
        lookPos.y = enemigo.transform.position.y;
        enemigo.transform.LookAt(lookPos);
    }

    public override void OnExit()
    {
        agent.isStopped = false;
    }

    private void ActualizarPosicion()
    {
        posicionObjetivo = EnemyManager.instance != null
            ? EnemyManager.instance.ObtenerPosicionParaRodear(enemigo, 6f)
            : enemigo.transform.position;
    }
}
