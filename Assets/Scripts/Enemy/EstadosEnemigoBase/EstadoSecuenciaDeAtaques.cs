using UnityEngine;
using UnityEngine.AI;

public class EstadoSecuenciaDeAtaques : EstadoBase
{
    private readonly NavMeshAgent agent;
    private readonly Transform jugador;
    private readonly SecuenciaAtaques[] ataquesEnSecuencia;
    private readonly Temporizador temporizadorEntreAtaques;
    private readonly float tiempoEntreAtaques;
    private float distanciaDeAtaque = 2f;

    private int indiceAtaques = 0;

    public EstadoSecuenciaDeAtaques(Enemigo enemigo, Animator animator, NavMeshAgent agent, Transform jugador, SecuenciaAtaques[] ataquesEnSecuencia, Temporizador temporizadorEntreAtaques, float tiempoEntreAtaques) : base(enemigo, animator)
    {
        this.agent = agent;
        this.jugador = jugador;
        this.ataquesEnSecuencia = ataquesEnSecuencia;
        this.tiempoEntreAtaques = tiempoEntreAtaques;
        this.temporizadorEntreAtaques = new Temporizador(tiempoEntreAtaques);
    }

    public override void OnEnter()
    {
        Debug.Log("Secuencia de ataques!!!");
        agent.isStopped = true;
        EjecutarAtaqueActual();
        temporizadorEntreAtaques.Empezar();
    }

    public override void Update()
    {
        // Siempre mirar al jugador
        Vector3 direccion = (jugador.position - enemigo.transform.position).normalized;
        direccion.y = 0f; // evitar inclinaciones verticales
        if (direccion != Vector3.zero)
        {
            enemigo.transform.rotation = Quaternion.Slerp(
                enemigo.transform.rotation,
                Quaternion.LookRotation(direccion),
                Time.deltaTime * 10f
            );
        }

        // Si estamos muy lejos, reposicionarnos al borde del rango
        float distanciaActual = Vector3.Distance(enemigo.transform.position, jugador.position);
        if (distanciaActual > distanciaDeAtaque + 0.1f)
        {
            Vector3 destino = jugador.position - direccion * distanciaDeAtaque;
            agent.isStopped = false;
            agent.SetDestination(destino);
            return; // aún no atacar hasta estar en rango
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        
        if (HaTerminadoAnimacionActual())
        {
            temporizadorEntreAtaques.Tick(Time.deltaTime);

            if (temporizadorEntreAtaques.HaFinalizado)
            {
                SiguienteAtaque();
                EjecutarAtaqueActual();
                temporizadorEntreAtaques.Empezar();
            }
        }
    }

    public override void OnExit()
    {
        agent.isStopped = false;
        agent.ResetPath();
        temporizadorEntreAtaques.Pausar();
        indiceAtaques = 0;
    }

    public void EjecutarAtaqueActual()
    {
        if (indiceAtaques >= ataquesEnSecuencia.Length) return;
        
        var overrideController = ataquesEnSecuencia[indiceAtaques].animatorOverride;
        animator.runtimeAnimatorController = overrideController;
        
        animator.CrossFade(SecuenceHash, duracionTransicion);
        //método para aplicar daño
    }

    private void SiguienteAtaque()
    {
        indiceAtaques++;

        if (indiceAtaques >= ataquesEnSecuencia.Length)
        {
            indiceAtaques = 0;
        }
    }

    private bool HaTerminadoAnimacionActual()
    {
        var info = animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("Secuencia") && info.normalizedTime >= 1f;
    }
    
    
}