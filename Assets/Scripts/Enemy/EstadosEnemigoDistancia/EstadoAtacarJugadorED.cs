using UnityEngine;
using UnityEngine.AI;

public class EstadoAtacarJugadorED : EstadoBaseED
{
    
    readonly NavMeshAgent agent;
    readonly Transform jugador;
    private readonly float tiempoEntreAtaques;
    private readonly float rangoMax;
    private readonly float rangoMin;
    private readonly Temporizador temporizadorAtaque;


    public EstadoAtacarJugadorED(EnemigoADistancia enemigo, Animator animator, NavMeshAgent agent, Transform jugador, float rangoMin, float rangoMax, float tiempoEntreAtaques) : base(enemigo, animator)
    {
        this.agent = agent;
        this.jugador = jugador;
        this.rangoMin = rangoMin;
        this.rangoMax = rangoMax;
        this.tiempoEntreAtaques = tiempoEntreAtaques;
        this.temporizadorAtaque = new Temporizador(tiempoEntreAtaques);
    }

    public override void OnEnter()
    {
        Debug.Log("Atacando!");
        agent.isStopped = false;
        temporizadorAtaque.Empezar();
    }

    public override void Update()
    {
        float distancia = Vector3.Distance(enemigo.transform.position, jugador.position);

        // 1. Reposicionarse si está muy cerca
        if (distancia < rangoMin)
        {
            Vector3 direccion = (enemigo.transform.position - jugador.position).normalized;
            Vector3 destino = enemigo.transform.position + direccion * (rangoMin - distancia + 1f);
            agent.SetDestination(destino);
            return;
        }

        // 2. Acercarse si está muy lejos
        if (distancia > rangoMax)
        {
            agent.SetDestination(jugador.position);
            return;
        }

        // 3. Dentro del rango óptimo
        agent.ResetPath();
        agent.isStopped = true;

        // Mirar al jugador
        Vector3 direccionJugador = jugador.position - enemigo.transform.position;
        direccionJugador.y = 0f;
        if (direccionJugador != Vector3.zero)
        {
            enemigo.transform.rotation = Quaternion.Slerp(
                enemigo.transform.rotation,
                Quaternion.LookRotation(direccionJugador),
                Time.deltaTime * 10f
            );
        }

        // Atacar
        temporizadorAtaque.Tick(Time.deltaTime);
        if (temporizadorAtaque.HaFinalizado)
        {
            animator.CrossFade(AttackHash, duracionTransicion);
            // enemigo.Disparar(); // implementas este método en Enemigo
            temporizadorAtaque.Empezar();
        }
    }

    public override void OnExit()
    {
        agent.isStopped = false;
    }
}