using UnityEngine;
using UnityEngine.AI;

public class EstadoStun : EstadoBase
{
    private readonly float duracionDeStun;
    private readonly NavMeshAgent agente;
    private readonly Temporizador tempStun;
    private readonly HealthComp vidaEnemigo;

    public bool stunFinalizado => tempStun.HaFinalizado;

    public EstadoStun(Enemigo enemigo, Animator animator, NavMeshAgent agente, HealthComp vidaEnemigo, float duracionDeStun) : base(enemigo, animator)
    {
        this.duracionDeStun = duracionDeStun;
        this.agente = agente;
        this.vidaEnemigo = vidaEnemigo;
        tempStun = new Temporizador(duracionDeStun);
    }
    
    public override void OnEnter()
    {
        Debug.Log("Stun!!!");
        animator.CrossFade(StunHash, duracionTransicion);
        agente.isStopped = true;
        tempStun.Reiniciar();
        tempStun.Empezar();
        vidaEnemigo.EntrarEnStun();
    }
    
    public override void Update()
    {
        if (!vidaEnemigo.EstaEnFinisher)
        {
            tempStun.Tick(Time.deltaTime);
        }
    }
    
    public override void OnExit()
    {
        agente.isStopped = false;
        vidaEnemigo.SalirDeStun();
        vidaEnemigo.RestablecerEstamina();
        vidaEnemigo.SetGolpesRecibidos(0);
    }
}