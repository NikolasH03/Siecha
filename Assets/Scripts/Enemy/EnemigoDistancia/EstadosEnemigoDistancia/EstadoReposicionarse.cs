using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado exclusivo del EnemigoDistancia.
/// Se activa cuando el jugador entra dentro de la distancia mínima de confort.
/// 
/// COMPORTAMIENTO:
/// Busca un punto en el NavMesh alejado del jugador y camina hacia él.
/// 
/// CONDICIONES DE SALIDA:
///   a) El enemigo llegó al destino.
///   b) El jugador volvió a estar lejos (distancia > distanciaMinima * 1.5).
///   c) Timeout de seguridad (atascado en geometría).
/// 
/// POR QUÉ HISTÉRESIS (factor 1.5):
/// Sin ella, si el jugador está exactamente en distanciaMinima, el enemigo
/// entraría y saldría del estado cada frame — el clásico flickering de FSM.
/// El factor 1.5 crea una zona de salida más amplia que la de entrada,
/// garantizando que el enemigo se aleje de verdad antes de considerar que
/// el problema está resuelto.
/// </summary>
public class EstadoReposicionarse : EstadoBase
{
    private readonly NavMeshAgent agent;

    private readonly float  distanciaMinima;
    private readonly float  distanciaObjetivo;
    private readonly float  velocidadHuida;

    private static readonly int MoverseHash = Animator.StringToHash("Running");

    private const float TIMEOUT_SEGUNDOS = 4f;

    public bool ReposicionamientoCompletado { get; private set; }

    private float timerSeguridad;

    public EstadoReposicionarse(
        Enemigo      enemigo,
        Animator     animator,
        NavMeshAgent agent,
        float        distanciaMinima,
        float        distanciaObjetivo,
        float        velocidadHuida)
        : base(enemigo, animator)
    {
        this.agent             = agent;
        this.distanciaMinima   = distanciaMinima;
        this.distanciaObjetivo = distanciaObjetivo;
        this.velocidadHuida    = velocidadHuida;
    }

    public override void OnEnter()
    {
        ReposicionamientoCompletado = false;
        timerSeguridad              = 0f;

        agent.speed     = velocidadHuida;
        agent.isStopped = false;

        agent.SetDestination(CalcularPuntoDeHuida());
        animator.Play(MoverseHash, 0, 0f);
    }

    public override void Update()
    {
        timerSeguridad += Time.deltaTime;

        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) { ReposicionamientoCompletado = true; return; }

        float distancia = Vector3.Distance(enemigo.transform.position, jugador.position);

        bool jugadorLejos = distancia > distanciaMinima * 1.5f;
        bool llegamos     = !agent.pathPending && agent.remainingDistance < 0.5f;
        bool timeout      = timerSeguridad >= TIMEOUT_SEGUNDOS;

        if (jugadorLejos || llegamos || timeout)
            ReposicionamientoCompletado = true;
    }

    public override void OnExit()
    {
        ReposicionamientoCompletado = false;
    }

    /// <summary>
    /// Calcula un punto en el NavMesh en dirección opuesta al jugador.
    /// Si el NavMesh no tiene posición válida en esa dirección exacta,
    /// hace un muestreo más cercano al enemigo como fallback.
    /// </summary>
    private Vector3 CalcularPuntoDeHuida()
    {
        Transform jugador = enemigo.JugadorActual;
        Vector3   dirHuida  = (enemigo.transform.position - jugador.position).normalized;
        Vector3   candidato = jugador.position + dirHuida * distanciaObjetivo;

        if (NavMesh.SamplePosition(candidato, out NavMeshHit hit, distanciaObjetivo * 0.5f, NavMesh.AllAreas))
            return hit.position;

        Vector3 fallback = enemigo.transform.position + dirHuida * (distanciaObjetivo * 0.5f);
        if (NavMesh.SamplePosition(fallback, out NavMeshHit hitFallback, 3f, NavMesh.AllAreas))
            return hitFallback.position;

        return enemigo.transform.position;
    }
}