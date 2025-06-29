using UnityEngine;
using UnityEngine.AI;

public class EstadoDeEsquivar : EstadoBase
{
    private readonly float velocidadDeEsquivo;
    private readonly float distanciaEsquivar;
    private readonly NavMeshAgent agent;
    private readonly HealthComp vidaEnemigo;

    private Vector3 movimientoDeEsquivado;
    private bool estaEsquivando;
    private float velocidadBase;

    public EstadoDeEsquivar(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo, float distanciaEsquivar, float velocidadDeEsquivo) : base(enemigo, animator)
    {
        this.agent  = agent;
        this.vidaEnemigo = vidaEnemigo;
        this.distanciaEsquivar  = distanciaEsquivar;
        this.velocidadDeEsquivo = velocidadDeEsquivo;
        this.velocidadBase = agent.speed;
    }

    public override void OnEnter()
    {
        Debug.Log("Esquivar!!!");
        animator.CrossFade(DodgeHash, duracionTransicion);
        vidaEnemigo.setEsquivando(true);
        
        // Elegir aleatoriamente izquierda o derecha
        Vector3 lateral = -enemigo.transform.right;
        movimientoDeEsquivado = enemigo.transform.position + lateral * distanciaEsquivar;

        agent.speed = velocidadDeEsquivo;
        agent.isStopped = false;
        agent.SetDestination(movimientoDeEsquivado);

        estaEsquivando = true;
    }

    public override void Update()
    {
        if(!estaEsquivando) return;
        
        float distanciaRestante = Vector3.Distance(enemigo.transform.position, movimientoDeEsquivado);
        
        if (distanciaRestante <= 0.1f)
        {
            agent.ResetPath();
            agent.isStopped = true;
            estaEsquivando = false;
        }

    }

    public override void OnExit()
    {
        agent.speed = velocidadBase;
        vidaEnemigo.setEsquivando(false);
        agent.isStopped = false;
    }
}