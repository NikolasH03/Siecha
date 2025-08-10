using UnityEngine;
using UnityEngine.AI;

public class EstadoRomperGuardia : EstadoBase
{
    private readonly HealthComp vidaEnemigo;
    private readonly NavMeshAgent agent;

    public EstadoRomperGuardia(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo) : base(enemigo, animator)
    {
        this.agent  = agent;
        this.vidaEnemigo  = vidaEnemigo;
    }

    public override void OnEnter()
    {
        Debug.Log("Guard Break!!!");
        animator.CrossFade(GuardBreakHash, duracionTransicion);
        agent.isStopped = true;
    }

    public override void Update()
    {
        
    }

    public override void OnExit()
    {
        agent.isStopped = false;
    }
}