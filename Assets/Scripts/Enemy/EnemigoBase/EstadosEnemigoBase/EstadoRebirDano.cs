using UnityEngine;
using UnityEngine.AI;

public class EstadoRebirDano : EstadoBase
{
    private readonly HealthComp   vidaEnemigo;
    protected readonly NavMeshAgent agente;
    
    private readonly float duracionMaxima;
    private float tiempoEnEstado;

    public EstadoRebirDano(Enemigo enemigo, Animator animator, NavMeshAgent agente,
                           HealthComp vidaEnemigo, float duracionDano)
        : base(enemigo, animator)
    {
        this.vidaEnemigo    = vidaEnemigo;
        this.agente         = agente;
        this.duracionMaxima = duracionDano;
    }

    public override void OnEnter()
    {
        tiempoEnEstado = 0f;

        enemigo.desactivarCollider();
        animator.CrossFade(DamageHash, duracionTransicion);
        vidaEnemigo.setRecibiendoDano(true);

        agente.velocity  = Vector3.zero;
        agente.isStopped = true;
    }

    public override void Update()
    {
        tiempoEnEstado += Time.deltaTime;
        
        bool animacionTermino = !vidaEnemigo.EstaSiendoDanado;
        bool tiempoAgotado    = tiempoEnEstado >= duracionMaxima;

        if (animacionTermino || tiempoAgotado)
        {
            if (tiempoAgotado && !animacionTermino)
            {
                Debug.LogWarning($"[{enemigo.name}] EstadoRebirDano: salida por timer de seguridad. " +
                                 $"Verifica el Animation Event 'TerminarDanoRecibido' en la animación de daño.");
                vidaEnemigo.TerminarDanoRecibido();
            }

            // Llamamos al nuevo método virtual en lugar de cambiar de estado directamente
            TransicionarAlSiguienteEstado(); 
        }
    }

    public override void OnExit()
    {
        agente.isStopped = false;
        agente.velocity  = Vector3.zero;
    }

    /// <summary>
    /// Encapsula la lógica de salida para que las variantes de enemigos (como el arquero)
    /// puedan decidir su propio comportamiento post-daño sin reescribir todo el Update.
    /// </summary>
    protected virtual void TransicionarAlSiguienteEstado()
    {
        if (enemigo.detectarJugador.SePuedeDetectarAlJugador())
            enemigo.CambiarAEstado<EstadoSeguirJugador>();
        else
            enemigo.CambiarAEstado<EstadoPatrullaEnemigo>();
    }
}