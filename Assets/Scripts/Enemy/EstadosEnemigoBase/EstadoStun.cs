using UnityEngine;
using UnityEngine.AI;

public class EstadoStun : EstadoBase
{
    private readonly NavMeshAgent agente;
    private readonly Temporizador tempStun;
    private readonly HealthComp vidaEnemigo;
    private bool yaTermino = false;

    public bool stunFinalizado => tempStun.HaFinalizado;

    public EstadoStun(Enemigo enemigo, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo, float duracionDeStun)
        : base(enemigo, animator)
    {
        this.agente = agente;
        this.vidaEnemigo = vidaEnemigo;
        tempStun = new Temporizador(duracionDeStun);
    }

    public override void OnEnter()
    {
        animator.CrossFade(StunHash, duracionTransicion);
        agente.isStopped = true;
        agente.velocity = Vector3.zero;

        tempStun.Reiniciar();
        tempStun.Empezar();

        vidaEnemigo.EntrarEnStun();
        yaTermino = false;
    }

    public override void Update()
    {
        if (yaTermino) return;

        if (!vidaEnemigo.EstaEnFinisher)
        {
            tempStun.Tick(Time.deltaTime);

            if (tempStun.HaFinalizado && !vidaEnemigo.EstaEnFinisher)
            {
                TerminarStun();
            }
        }
    }

    private void TerminarStun()
    {
        if (yaTermino) return;

        yaTermino = true;

        vidaEnemigo.SalirDeStun();
        vidaEnemigo.RestablecerEstamina();
        vidaEnemigo.SetGolpesRecibidos(0);
        agente.isStopped = false;

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
        agente.isStopped = false;
        agente.velocity = Vector3.zero;

        if (!yaTermino && !vidaEnemigo.EstaEnFinisher)
        {
            vidaEnemigo.SalirDeStun();
            vidaEnemigo.RestablecerEstamina();
            vidaEnemigo.SetGolpesRecibidos(0);
        }
    }
}