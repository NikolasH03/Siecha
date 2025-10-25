using UnityEngine;
using UnityEngine.AI;

public class EstadoRodearJugador : EstadoBase
{
    private readonly NavMeshAgent agent;

    private float distanciaDeRodeo = 4f;
    private float velocidadAngular = 50f;

    public EstadoRodearJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent) : base(enemigo, animator)
    {
        this.agent = agent;
    }

    public override void OnEnter()
    {
        Debug.Log("Rodeando Jugador!!!");
        animator.CrossFade(WalkingHash, duracionTransicion);
    }

    public override void Update()
    {
        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) return;

        Vector3 direccion = (enemigo.transform.position - jugador.position).normalized;
        Vector3 objetivo = jugador.position + Quaternion.Euler(0, velocidadAngular * Time.deltaTime, 0) * direccion * distanciaDeRodeo;
        agent.SetDestination(objetivo);
        enemigo.transform.LookAt(jugador);
    }

    public override void OnExit()
    {
        
    }
}