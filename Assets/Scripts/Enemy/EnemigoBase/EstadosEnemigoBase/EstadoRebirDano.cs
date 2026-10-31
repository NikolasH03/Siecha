using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Reacción al golpe: el enemigo se detiene hasta que termina la animación de daño
/// (evento 'TerminarDanoRecibido') o vence un timer de seguridad.
/// </summary>
public class EstadoRebirDano : EstadoBase
{
    protected readonly NavMeshAgent agente;
    private readonly HealthComp vidaEnemigo;
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
        if (!animacionTermino && !tiempoAgotado) return;

        if (!animacionTermino)
        {
            Debug.LogWarning($"[{enemigo.name}] EstadoRebirDano: salida por timer de seguridad. " +
                             "Verifica el Animation Event 'TerminarDanoRecibido' en la animación de daño.");
            vidaEnemigo.TerminarDanoRecibido();
        }

        TransicionarAlSiguienteEstado();
    }

    public override void OnExit()
    {
        agente.isStopped = false;
        agente.velocity  = Vector3.zero;
    }

    /// <summary>
    /// Las variantes (p. ej. el arquero) pueden sobreescribirlo para decidir su propio comportamiento post-daño.
    /// Por defecto lo decide Enemigo: sin slot de ataque no persigue, rodea.
    /// </summary>
    protected virtual void TransicionarAlSiguienteEstado() => enemigo.VolverAComportamientoBase();
}