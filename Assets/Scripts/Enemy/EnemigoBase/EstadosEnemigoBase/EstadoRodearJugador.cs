using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Orbita al jugador sin atacar mientras espera un slot de ataque.
/// La posición objetivo la reparte EnemyManager y se refresca cada pocos segundos.
/// </summary>
public class EstadoRodearJugador : EstadoBase
{
    private const float DistanciaRodeo          = 6f;
    // La deriva orbital del manager se aplica una vez por actualización: este intervalo fija la velocidad orbital.
    private const float IntervaloActualizacion  = 1f;

    private readonly NavMeshAgent agent;
    private readonly HealthComp   salud;

    private Vector3 posicionObjetivo;
    private float   tiempoHastaActualizar;

    public EstadoRodearJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent)
        : base(enemigo, animator)
    {
        this.agent = agent;
        salud      = enemigo.GetHealthComp();
    }

    public override void OnEnter()
    {
        animator.CrossFade(WalkingHash, duracionTransicion);
        ActualizarPosicion();
        tiempoHastaActualizar = IntervaloActualizacion;
    }

    public override void Update()
    {
        // Bloqueando o esquivando: quieto para que la defensa funcione.
        if (salud.getBloqueando() || salud.EstaEsquivando)
        {
            agent.isStopped = true;
            return;
        }
        agent.isStopped = false;

        Transform jugador = enemigo.JugadorActual;

        if (jugador == null || !enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        // Con slot concedido primero se acerca; Seguir pasa a Atacar al entrar en rango.
        if (enemigo.EstaAtacando())
        {
            enemigo.CambiarAEstado<EstadoSeguirJugador>();
            return;
        }

        tiempoHastaActualizar -= Time.deltaTime;
        if (tiempoHastaActualizar <= 0f)
        {
            ActualizarPosicion();
            tiempoHastaActualizar = IntervaloActualizacion;
        }

        agent.SetDestination(posicionObjetivo);
        MirarAlJugador(jugador);
    }

    public override void OnExit() => agent.isStopped = false;

    private void ActualizarPosicion()
    {
        // Sin manager, se queda donde está.
        posicionObjetivo = EnemyManager.instance != null
            ? EnemyManager.instance.ObtenerPosicionParaRodear(enemigo, DistanciaRodeo)
            : enemigo.transform.position;
    }

    private void MirarAlJugador(Transform jugador)
    {
        Vector3 mirar = jugador.position;
        mirar.y = enemigo.transform.position.y;
        enemigo.transform.LookAt(mirar);
    }
}