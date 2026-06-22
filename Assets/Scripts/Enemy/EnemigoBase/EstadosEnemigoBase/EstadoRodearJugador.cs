using UnityEngine;
using UnityEngine.AI;

public class EstadoRodearJugador : EstadoBase
{
    private readonly NavMeshAgent agent;
    private Vector3 posicionObjetivo;
    private float tiempoHastaActualizar;
    private float intervaloActualizacion = 2f;

    public EstadoRodearJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent)
        : base(enemigo, animator)
    {
        this.agent = agent;
    }

    public override void OnEnter()
    {
        animator.CrossFade(WalkingHash, duracionTransicion);
        ActualizarPosicion();
        tiempoHastaActualizar = intervaloActualizacion;
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;

        if (enemigo.GetHealthComp().getBloqueando() || enemigo.GetHealthComp().EstaEsquivando)
        {
            agent.isStopped = true; // Detener el movimiento para que la defensa funcione
            return; 
        }
    
        agent.isStopped = false;
        if (jugador == null)
        {
            Debug.LogWarning($"[{enemigo.name}] Jugador null en EstadoRodear, volviendo a patrullar");
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        if (!enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            Debug.Log($"[{enemigo.name}] Perdio deteccion del jugador, volviendo a patrullar");
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }
        if (enemigo.EstaAtacando()) 
        {
            enemigo.CambiarAEstado<EstadoAtacarJugador>();
            return;
        }

        tiempoHastaActualizar -= Time.deltaTime;

        if (tiempoHastaActualizar <= 0f)
        {
            ActualizarPosicion();
            tiempoHastaActualizar = intervaloActualizacion;
        }

        agent.SetDestination(posicionObjetivo);

        // Siempre mirar al jugador
        Vector3 lookPosition = jugador.position;
        lookPosition.y = enemigo.transform.position.y;
        enemigo.transform.LookAt(lookPosition);
    }

    private void ActualizarPosicion()
    {
        if (EnemyManager.instance != null)
        {
            // Usar distancia del radio de deteccion automotica como referencia
            float distanciaRodeo = enemigo.detectarJugador != null ? 6f : 6f;
            posicionObjetivo = EnemyManager.instance.ObtenerPosicionParaRodear(enemigo, distanciaRodeo);
        }
        else
        {
            // Fallback: quedarse donde esta
            posicionObjetivo = enemigo.transform.position;
        }
    }

    public override void OnExit()
    {
        agent.isStopped = false;
    }
}