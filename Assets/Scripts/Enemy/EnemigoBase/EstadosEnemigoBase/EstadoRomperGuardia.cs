using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Animación de guardia rota: el enemigo queda quieto e invulnerable un instante antes de pasar a stun
/// (la transición la define Enemigo con guardBreakFinalizado).
/// </summary>
public class EstadoRomperGuardia : EstadoBase
{
    // TODO: mover a EnemyStats como el resto de parámetros de enemigo.
    private const float DuracionAnimacion = 0.5f;

    private readonly NavMeshAgent agent;
    private readonly Temporizador tempGuardBreak;

    public bool guardBreakFinalizado => tempGuardBreak.HaFinalizado;

    public EstadoRomperGuardia(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo)
        : base(enemigo, animator)
    {
        this.agent     = agent;
        tempGuardBreak = new Temporizador(DuracionAnimacion);
    }

    public override void OnEnter()
    {
        enemigo.ActivarInvulnerabilidad();
        animator.CrossFade(GuardBreakHash, duracionTransicion);
        agent.isStopped = true;

        tempGuardBreak.Reiniciar();
        tempGuardBreak.Empezar();
    }

    public override void Update() => tempGuardBreak.Tick(Time.deltaTime);

    public override void OnExit()
    {
        agent.isStopped = false;
        enemigo.DesactivarInvulnerabilidad();
    }
}