using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado exclusivo del EnemigoDistancia.
/// 
/// FASES INTERNAS (sin estados separados, porque son demasiado cortas
/// y la transición entre ellas no necesita lógica de la FSM):
/// 
///   1. APUNTANDO: el enemigo se detiene, rota hacia el jugador y espera
///      tiempoApuntado segundos. La animación de apuntado corre aquí.
/// 
///   2. DISPARANDO: instancia el proyectil desde el spawnPoint apuntando
///      al jugador. Dura un frame (es un evento puntual).
/// 
///   3. COOLDOWN: el enemigo puede moverse libremente pero no dispara.
///      Cuando termina, el estado se marca como completado y la FSM
///      de EnemigoDistancia transiciona a otro estado.
/// 
/// POR QUÉ FASES INTERNAS Y NO ESTADOS SEPARADOS:
/// Las tres fases comparten el mismo contexto (mismo jugador apuntado,
/// mismo proyectil a disparar). Separarlas en estados distintos requeriría
/// pasar ese contexto entre estados, que es más complejo sin beneficio real.
/// </summary>
public class EstadoApuntarYDisparar : EstadoBase
{
    private readonly NavMeshAgent       agent;
    private readonly EnemigoDistancia   enemigoDistancia;

    private readonly float tiempoApuntado;
    private readonly float tiempoCooldown;

    // Hashes pre-calculados — mismo patrón que EstadoBase.
    // static readonly porque el hash de un nombre nunca cambia entre instancias.
    private static readonly int ApuntarHash  = Animator.StringToHash("Aim");
    private static readonly int DispararHash = Animator.StringToHash("Shoot");

    private enum Fase { Apuntando, Disparando, Cooldown }
    private Fase  faseActual;
    private float timerFase;

    /// <summary>
    /// La FSM de EnemigoDistancia consulta este flag para saber cuándo salir
    /// del estado sin interrumpir el ciclo de disparo.
    /// </summary>
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
                {
                    faseActual = Fase.Disparando;
                    timerFase  = 0f;
                }
                break;

            case Fase.Disparando:
                enemigoDistancia.Disparar();
                animator.Play(DispararHash, 0, 0f);
                agent.isStopped = false;
                faseActual      = Fase.Cooldown;
                timerFase       = 0f;
                // Volver a Idle durante el cooldown: el arquero no se queda
                // congelado en el último frame de la animación de disparo.
                // La animación de disparo termina sola; esta llamada garantiza
                // que haya un estado de destino cuando lo haga.
                animator.CrossFade(IddleHash, duracionTransicion);
                break;

            case Fase.Cooldown:
                if (timerFase >= tiempoCooldown)
                    DisparoCompletado = true;
                break;
        }
    }

    public override void OnExit()
    {
        agent.isStopped   = false;
        DisparoCompletado = false;
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
            Time.deltaTime * 8f
        );
    }
}