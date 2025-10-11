using UnityEngine;
using UnityEngine.AI;

public class EstadoRomperGuardia : EstadoBase
{
    private readonly HealthComp vidaEnemigo;
    private readonly NavMeshAgent agent;

    private readonly Temporizador tempGuardBreak;
    
    public bool guardBreakFinalizado => tempGuardBreak.HaFinalizado;

    public EstadoRomperGuardia(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo) : base(enemigo, animator)
    {
        this.agent  = agent;
        this.vidaEnemigo  = vidaEnemigo;
        this.tempGuardBreak = new Temporizador(0.5f);
    }

    public override void OnEnter()
    {
        Debug.Log("Guard Break!!!");
        animator.CrossFade(GuardBreakHash, duracionTransicion);
        tempGuardBreak.Reiniciar();
        tempGuardBreak.Empezar();
    }

    public override void Update()
    {
        tempGuardBreak.Tick(Time.deltaTime);
    }

    public override void OnExit()
    {
        
    }
}