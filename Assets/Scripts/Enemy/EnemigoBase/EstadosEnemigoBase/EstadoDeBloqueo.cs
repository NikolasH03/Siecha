using UnityEngine;
using UnityEngine.AI;

public class EstadoDeBloqueo : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly HealthComp vidaEnemigo;
    private float tiempoEnBloqueo = 0f;

    public EstadoDeBloqueo(Enemigo enemigo, Animator animator, NavMeshAgent agent, HealthComp vidaEnemigo)
        : base(enemigo, animator)
    {
        this.agent = agent;
        this.vidaEnemigo = vidaEnemigo;
    }

    public override void OnEnter()
    {
        vidaEnemigo.setBloqueado(true);
        animator.CrossFade(BlockHash, duracionTransicion);
        agent.isStopped = true;
        tiempoEnBloqueo = 0f;
    }

    public override void Update()
    {
        tiempoEnBloqueo += Time.deltaTime;

        // Salir si el jugador dejó de atacar
        if (!enemigo.JugadorEstaAtacando())
        {
            TerminarBloqueo();
            return;
        }

        // Timeout de seguridad (5 segundos máximo)
        if (tiempoEnBloqueo >= 5f)
        {
            TerminarBloqueo();
            return;
        }
    }

    private void TerminarBloqueo()
    {
        vidaEnemigo.setBloqueado(false);
        agent.isStopped = false;

        if (enemigo.detectarJugador.SePuedeDetectarAlJugador())
        {
            enemigo.CambiarAEstado<EstadoSeguirJugador>();
        }
        else
        {
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
        }
    }

    public override void OnExit()
    {
        agent.isStopped = false;
        vidaEnemigo.setBloqueado(false);
    }
}