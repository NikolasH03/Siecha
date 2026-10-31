using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado exclusivo de EnemigoDistancia. Tres fases internas (comparten el mismo contexto,
/// por eso no son estados separados de la FSM):
///   1. Apuntando: se detiene, rota hacia el jugador y espera tiempoApuntado.
///   2. Disparando: instancia el proyectil (dura un frame).
///   3. Cooldown: no dispara; al terminar marca DisparoCompletado y pasa a rodear.
/// </summary>
public class EstadoApuntarYDisparar : EstadoBase
{
    private const float VelocidadRotacion = 8f;

    // static readonly: el hash de un nombre no cambia entre instancias.
    private static readonly int ApuntarHash  = Animator.StringToHash("Aim");
    private static readonly int DispararHash = Animator.StringToHash("Shoot");

    private enum Fase { Apuntando, Disparando, Cooldown }

    private readonly NavMeshAgent     agent;
    private readonly EnemigoDistancia enemigoDistancia;
    private readonly float            tiempoApuntado;
    private readonly float            tiempoCooldown;

    private Fase  faseActual;
    private float timerFase;

    /// <summary>EnemigoDistancia lo consulta para no interrumpir el ciclo de disparo.</summary>
    public bool DisparoCompletado { get; private set; }

    public EstadoApuntarYDisparar(
        Enemigo          enemigo,
        Animator         animator,
        NavMeshAgent     agent,
        EnemigoDistancia enemigoDistancia,
        float            tiempoApuntado,
        float            tiempoCooldown)
        : base(enemigo, animator)
    {
        this.agent            = agent;
        this.enemigoDistancia = enemigoDistancia;
        this.tiempoApuntado   = tiempoApuntado;
        this.tiempoCooldown   = tiempoCooldown;
    }

    public override void OnEnter()
    {
        DisparoCompletado = false;
        faseActual        = Fase.Apuntando;
        timerFase         = 0f;

        agent.isStopped = true;
        animator.Play(ApuntarHash, 0, 0f);
    }

    public override void Update()
    {
        timerFase += Time.deltaTime;
        RotarHaciaJugador();

        switch (faseActual)
        {
            case Fase.Apuntando:
                if (timerFase >= tiempoApuntado)
                    CambiarFase(Fase.Disparando);
                break;

            case Fase.Disparando:
                Disparar();
                CambiarFase(Fase.Cooldown);
                break;

            case Fase.Cooldown:
                if (timerFase >= tiempoCooldown)
                {
                    DisparoCompletado = true;
                    enemigoDistancia.CambiarAEstado<EstadoRodearDistancia>();
                }
                break;
        }
    }

    public override void OnExit()
    {
        agent.isStopped   = false;
        DisparoCompletado = false;

        // Libera el slot de disparo en el EnemyManager y limpia la bandera 'atacando'.
        enemigoDistancia.FinalizarAtaqueDistancia();
    }

    private void CambiarFase(Fase nueva)
    {
        faseActual = nueva;
        timerFase  = 0f;
    }

    private void Disparar()
    {
        enemigoDistancia.Disparar();
        animator.Play(DispararHash, 0, 0f);
        agent.isStopped = false;

        // Asegura un estado de destino para que el arquero no quede congelado en el último frame del disparo.
        animator.CrossFade(IddleHash, duracionTransicion);
    }

    private void RotarHaciaJugador()
    {
        Transform jugador = enemigo.JugadorActual;
        if (jugador == null) return;

        Vector3 dir = jugador.position - enemigo.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        enemigo.transform.rotation = Quaternion.Slerp(
            enemigo.transform.rotation,
            Quaternion.LookRotation(dir.normalized),
            Time.deltaTime * VelocidadRotacion
        );
    }
}