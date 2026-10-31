using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo aturdido (tras romperle la guardia): inmóvil y vulnerable a finisher.
/// Al acabar el tiempo recupera stamina y vuelve a su comportamiento base.
/// </summary>
public class EstadoStun : EstadoBase
{
    private readonly NavMeshAgent agente;
    private readonly Temporizador tempStun;
    private readonly HealthComp   vidaEnemigo;

    private bool yaTermino;

    public bool stunFinalizado => tempStun.HaFinalizado;

    public EstadoStun(Enemigo enemigo, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo, float duracionDeStun)
        : base(enemigo, animator)
    {
        this.agente      = agente;
        this.vidaEnemigo = vidaEnemigo;
        tempStun         = new Temporizador(duracionDeStun);
    }

    public override void OnEnter()
    {
        animator.CrossFade(StunHash, duracionTransicion);
        agente.isStopped = true;
        agente.velocity  = Vector3.zero;

        tempStun.Reiniciar();
        tempStun.Empezar();

        vidaEnemigo.EntrarEnStun();
        yaTermino = false;
    }

    public override void Update()
    {
        // En finisher el temporizador se congela: el stun no debe expirar.
        if (yaTermino || vidaEnemigo.EstaEnFinisher) return;

        tempStun.Tick(Time.deltaTime);
        if (tempStun.HaFinalizado) TerminarStun();
    }

    public override void OnExit()
    {
        agente.isStopped = false;
        agente.velocity  = Vector3.zero;

        // Salida anticipada (p. ej. por daño): se restaura igual que al terminar, salvo en finisher.
        if (!yaTermino && !vidaEnemigo.EstaEnFinisher)
            RecuperarseDelStun();
    }

    private void TerminarStun()
    {
        yaTermino = true;

        RecuperarseDelStun();
        agente.isStopped = false;

        // Sin slot de ataque no persigue al jugador: lo decide Enemigo.
        enemigo.VolverAComportamientoBase();
    }

    private void RecuperarseDelStun()
    {
        vidaEnemigo.SalirDeStun();
        vidaEnemigo.RestablecerEstamina();
        vidaEnemigo.SetGolpesRecibidos(0);
    }
}