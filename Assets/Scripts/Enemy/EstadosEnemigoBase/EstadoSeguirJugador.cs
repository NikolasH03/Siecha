using UnityEngine;
using UnityEngine.AI;

public class EstadoSeguirJugador : EstadoBase
{
    readonly NavMeshAgent agent;

    private float velocidadBase;
    private float velocidadPersecucion;
    public EstadoSeguirJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, float velocidadPersecucion) : base(enemigo, animator)
    {
        this.agent = agent;
        this.velocidadBase = agent.speed;
        this.velocidadPersecucion = velocidadPersecucion;
    }

    public override void OnEnter()
    {
        Debug.Log("Perseguir");
        animator.CrossFade(RunningHash, duracionTransicion);
        agent.speed = velocidadPersecucion;
    }

    public override void Update()
    {
        agent.SetDestination(enemigo.JugadorActual.position);
    }

    public override void OnExit()
    {
        agent.speed = velocidadBase;
    }
}