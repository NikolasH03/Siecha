using UnityEngine;
using UnityEngine.AI;

public class EstadoPatrullaED : EstadoBaseED
{
    private readonly NavMeshAgent agent;
    private readonly float radioDePatrulla;
    readonly Vector3 posicionInicialDelEnemigo;
    
    
    private readonly float tiempoEspera;
    private float tiempoEsperando;
    private bool estaEsperando = false;


    public EstadoPatrullaED(EnemigoADistancia enemigo, Animator animator, NavMeshAgent agente, float radioDePatrulla, float tiempoEspera) : base(enemigo, animator)
    {
        this.agent = agente;
        this.posicionInicialDelEnemigo = enemigo.transform.position;
        this.radioDePatrulla = radioDePatrulla;
        this.tiempoEspera = tiempoEspera;
        
    }

    public override void OnEnter()
    {
        Debug.Log("Estado de patrulla");
        animator.CrossFade(WalkingHash, duracionTransicion);
        
    }

    public override void Update()
    {
        if (estaEsperando)
        {
            tiempoEsperando -= Time.deltaTime;
            if (tiempoEsperando <= 0)
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

    void IniciarEspera()
    {
        estaEsperando = true;
        tiempoEsperando = tiempoEspera;
        agent.isStopped = true;
        animator.CrossFade(IddleHash, duracionTransicion);

    }

    void EstablecerNuevoDestino()
    {
        agent.isStopped = false;
        animator.CrossFade(WalkingHash, duracionTransicion);
        
        var direccionAleatoria = Random.insideUnitSphere * radioDePatrulla;
        direccionAleatoria += posicionInicialDelEnemigo;
        NavMeshHit hit;
        NavMesh.SamplePosition(direccionAleatoria, out hit, radioDePatrulla, 1);
        var posicionFinalDestino = hit.position;

        agent.SetDestination(posicionFinalDestino);
    }

    bool HaLlegadoAlDestino()
    {
        return !agent.pathPending
               && agent.remainingDistance <= agent.stoppingDistance
               && (agent.hasPath || agent.velocity.magnitude == 0f);
    }
}