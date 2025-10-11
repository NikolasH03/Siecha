using UnityEngine;
using UnityEngine.AI;

public class EstadoSeguirJugador : EstadoBase
{
    readonly NavMeshAgent agent;
    readonly Transform jugador;

    private float velocidadBase;
    private float velocidadPersecucion;
    public EstadoSeguirJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, Transform juagdor, float velocidadPersecucion) : base(enemigo, animator)
    {
        this.agent = agent;
        this.jugador = juagdor;
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
        agent.SetDestination(jugador.position);
    }

    public override void OnExit()
    {
        agent.speed = velocidadBase;
    }
}