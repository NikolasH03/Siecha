using UnityEngine;
using UnityEngine.AI;

public class EstadoAtacarJugador : EstadoBase
{
    readonly NavMeshAgent agent;
    readonly Transform jugador;
    readonly float rangoDeAtaque;
    
    
    public EstadoAtacarJugador(Enemigo enemigo, Animator animator, NavMeshAgent agent, Transform jugador, float rangoDeAtaque) : base(enemigo, animator)
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
        Vector3 direccionAlJugador = (jugador.position - enemigo.transform.position).normalized;
        float distanciaDeseada = rangoDeAtaque;

        Vector3 destino = jugador.position - direccionAlJugador * distanciaDeseada;

        agent.SetDestination(destino);

        // Mirar hacia el jugador mientras se acerca
        Vector3 lookDir = jugador.position - enemigo.transform.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            enemigo.transform.rotation = Quaternion.Slerp(
                enemigo.transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 10f
            );
        }

        enemigo.Atacar();
    }

    
    
}