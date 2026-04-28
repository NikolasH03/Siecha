using UnityEngine;
using UnityEngine.AI;
public class EstadoRebirDano : EstadoBase
{
    private readonly HealthComp vidaEnemigo;
    private readonly NavMeshAgent agente;

    public EstadoRebirDano(Enemigo enemigo, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo, float duracionDano)
        : base(enemigo, animator)
    {
        this.vidaEnemigo = vidaEnemigo;
        this.agente = agente;
    }

    public override void OnEnter()
    {
        enemigo.desactivarCollider();
        animator.CrossFade(DamageHash, duracionTransicion);
        vidaEnemigo.setRecibiendoDano(true);
        agente.isStopped = true;
        agente.velocity = Vector3.zero;

    }

    public override void Update()
    {
        if (!vidaEnemigo.EstaSiendoDanado)
        {
            if (enemigo.detectarJugador.SePuedeDetectarAlJugador())
            {
                enemigo.CambiarAEstado<EstadoSeguirJugador>();
            }
            else
            {
                enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
            }
        }
    }

    public override void OnExit()
    {
        agente.isStopped = false;
        agente.velocity = Vector3.zero;
    }
}