using UnityEngine;
using UnityEngine.AI;

public class EstadoAtacarJugador : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly Transform jugador;
    private readonly float rangoDeAtaque;

    public EstadoAtacarJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, 
        Transform jugador, float rangoDeAtaque) 
        : base(enemigo, animator)
    {
        this.agent = agent;
        this.jugador = jugador;
        this.rangoDeAtaque = rangoDeAtaque;
    }

    public override void OnEnter()
    {
        Debug.Log("Atacando!");
        animator.CrossFade(AttackHash, duracionTransicion);
    }

    public override void Update()
    {
        // Calcular posición de ataque
        Vector3 direccionAlJugador = (jugador.position - enemigo.transform.position).normalized;
        float distanciaDeseada = rangoDeAtaque;
        Vector3 destino = jugador.position - direccionAlJugador * distanciaDeseada;
        
        // Moverse hacia la posición de ataque
        agent.SetDestination(destino);

        // Mirar hacia el jugador usando LookAt (más simple y confiable)
        Vector3 lookPosition = jugador.position;
        lookPosition.y = enemigo.transform.position.y; // Mantener la misma altura Y
        enemigo.transform.LookAt(lookPosition);

        // Ejecutar ataque
        enemigo.Atacar();
    }
}