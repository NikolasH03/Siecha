using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Patrulla: camina a puntos aleatorios alrededor de su posición inicial, con una pausa al llegar a cada uno.
/// </summary>
public class EstadoPatrullaEnemigo : EstadoBase
{
    private const int AreaCaminable = 1; // máscara de áreas del NavMesh: solo "Walkable"

    private readonly NavMeshAgent agent;
    private readonly float   radioDePatrulla;
    private readonly float   tiempoEspera;
    private readonly Vector3 posicionInicialDelEnemigo;

    private float tiempoEsperando;
    private bool  estaEsperando;

    public EstadoPatrullaEnemigo(Enemigo enemigo, Animator animator, NavMeshAgent agente, float radioDePatrulla, float tiempoEspera)
        : base(enemigo, animator)
    {
        agent                     = agente;
        posicionInicialDelEnemigo = enemigo.transform.position;
        this.radioDePatrulla      = radioDePatrulla;
        this.tiempoEspera         = tiempoEspera;
    }

    public override void OnEnter() => animator.CrossFade(WalkingHash, duracionTransicion);

    public override void Update()
    {
        if (estaEsperando)
        {
            tiempoEsperando -= Time.deltaTime;
            if (tiempoEsperando <= 0f)
            {
                estaEsperando = false;
                EstablecerNuevoDestino();
            }
        }
        else if (HaLlegadoAlDestino())
        {
            IniciarEspera();
        }
    }

    private void IniciarEspera()
    {
        estaEsperando   = true;
        tiempoEsperando = tiempoEspera;
        agent.isStopped = true;
        animator.CrossFade(IddleHash, duracionTransicion);
    }

    private void EstablecerNuevoDestino()
    {
        agent.isStopped = false;
        animator.CrossFade(WalkingHash, duracionTransicion);

        Vector2 offset    = Random.insideUnitCircle * radioDePatrulla;
        Vector3 candidato = posicionInicialDelEnemigo + new Vector3(offset.x, 0f, offset.y);

        // Si no hay NavMesh cerca, hit.position queda en (0,0,0): no se debe usar como destino.
        if (NavMesh.SamplePosition(candidato, out NavMeshHit hit, radioDePatrulla, AreaCaminable))
            agent.SetDestination(hit.position);
    }

    private bool HaLlegadoAlDestino() =>
        !agent.pathPending
        && agent.remainingDistance <= agent.stoppingDistance
        && (agent.hasPath || agent.velocity.magnitude == 0f);
}