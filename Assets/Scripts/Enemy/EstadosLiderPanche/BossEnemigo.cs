using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Configuración de armas para una fase del jefe.
/// Separa la responsabilidad visual (GameObjects) de la de hitbox (Colliders).
/// Soporta cualquier número de armas por fase — fase 1 tiene 2 cuchillos,
/// fase 2 una lanza + escudo, etc. Sin cambiar código.
/// </summary>
[System.Serializable]
public class ConfiguracionArmaFase
{
    [Tooltip("GameObjects visuales a activar en esta fase (meshes, VFX, etc.)")]
    public GameObject[] objetosVisuales;

    [Tooltip("Colliders de daño activos durante los ataques de esta fase.")]
    public Collider[] collidersAtaque;
}

public class BossEnemigo : Enemigo
{
    [Header("Boss Stats")]
    [SerializeField] private BossStats bossStats;

    [Header("Armas por Fase")]
    [Tooltip("Fase 1: dos cuchillos. Fase 2: lanza+escudo. Fase 3: macana.")]
    [SerializeField] private ConfiguracionArmaFase[] armasPorFase = new ConfiguracionArmaFase[3];

    // Estado actual
    private int  faseActual          = 1;
    private bool cinematicaEnCurso   = false;

    // Colliders activos de la fase actual — BossEnemigo gestiona la lista
    // y sobreescribe activarCollider/desactivarCollider de la clase base.
    private Collider[] collidersActivos = new Collider[0];

    private BossEstadoMuerte bossEstadoMuerte;

    // ─── Eventos para BossBattleState ─────────────────────────────────────────
    public static event Action<BossEnemigo, int> OnCambioFase;
    public static event Action<BossEnemigo>      OnBossPrepararse;

    public int FaseActual => faseActual;

    // ─── Inicialización ───────────────────────────────────────────────────────

    protected override void Start()
    {
        if (bossStats == null)
        {
            Debug.LogError($"[{name}] BossEnemigo no tiene BossStats asignado.");
            enabled = false;
            return;
        }

        if (armasPorFase.Length < 3)
            Debug.LogWarning($"[{name}] armasPorFase debería tener 3 entradas (una por fase).");

        ActualizarStats(bossStats.statsFaseUno);
        InicializarBase();
        InicializarEstadosReactivos();
        ConfigurarTransicionesReactivas();
        InicializarEstadoInicial();

        ActivarArmasDeFase(1);
        OnBossPrepararse?.Invoke(this);
    }

    protected override void InicializarEstadosReactivos()
    {
        base.InicializarEstadosReactivos();
        bossEstadoMuerte = new BossEstadoMuerte(this, Animator, Agent, VidaEnemigo);
        RegistrarEstadoEnCache(typeof(BossEstadoMuerte), bossEstadoMuerte);
    }

    protected override void ConfigurarTransicionesReactivas()
    {
        DesdeCualquier(bossEstadoMuerte,  new FuncPredicate(() => VidaEnemigo.EstaMuerto));
        DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => VidaEnemigo.EnemigoFueDanado()));

        Desde(estadoBloqueo,      estadoRompeGuardia, new FuncPredicate(() => VidaEnemigo.EnGuardBreak));
        Desde(estadoRompeGuardia, estadoStun,         new FuncPredicate(() => estadoRompeGuardia.guardBreakFinalizado));
    }

    protected override void InicializarEstadoInicial()
    {
        CambiarAEstado<EstadoRodearJugador>();
    }

    // ─── Override de EvaluarComportamiento ───────────────────────────────────

    public override void EvaluarComportamiento()
    {
        if (cinematicaEnCurso) return;
        VerificarUmbralesDeFase();
        if (cinematicaEnCurso) return;
        base.EvaluarComportamiento();
    }

    // ─── Transiciones de fase ─────────────────────────────────────────────────

    private void VerificarUmbralesDeFase()
    {
        if (VidaEnemigo == null || VidaEnemigo.EstaMuerto) return;
        float vida = VidaEnemigo.GetVidaNormalizada();

        if (faseActual == 1 && vida <= bossStats.umbralFaseDos)
            IniciarTransicionFase(2);
        else if (faseActual == 2 && vida <= bossStats.umbralFaseTres)
            IniciarTransicionFase(3);
    }

    private void IniciarTransicionFase(int nuevaFase)
    {
        if (cinematicaEnCurso || faseActual >= nuevaFase) return;
        cinematicaEnCurso = true;
        PausarParaCinematica();
        OnCambioFase?.Invoke(this, nuevaFase);
    }

    // ─── API para BossBattleState ─────────────────────────────────────────────

    public void ReanudarDesdeFase(int nuevaFase)
    {
        faseActual = nuevaFase;
        EnemyStats nuevosStats = bossStats.ObtenerStatsDeFase(nuevaFase);
        ActualizarStats(nuevosStats);
        VidaEnemigo.ActualizarParametrosDeCombate(nuevosStats);
        ActivarArmasDeFase(nuevaFase);
        cinematicaEnCurso = false;
        ReanudarDespuesDeCinematica();
        Debug.Log($"[{name}] Fase {nuevaFase} — {nuevosStats.nombreEnemigo}");
    }

    private void PausarParaCinematica()
    {
        Agent.velocity  = Vector3.zero;
        Agent.isStopped = true;
        TerminarAtaque();
    }

    private void ReanudarDespuesDeCinematica()
    {
        Agent.isStopped = false;
        CambiarAEstado<EstadoSeguirJugador>();
    }

    // ─── Armas por fase ───────────────────────────────────────────────────────

    private void ActivarArmasDeFase(int fase)
    {
        // Desactivar todos los objetos visuales de todas las fases
        foreach (var configArma in armasPorFase)
        {
            if (configArma?.objetosVisuales == null) continue;
            foreach (var obj in configArma.objetosVisuales)
                if (obj != null) obj.SetActive(false);
        }

        // Activar los de la fase solicitada (índice base 0, fase base 1)
        int idx = fase - 1;
        if (idx < 0 || idx >= armasPorFase.Length || armasPorFase[idx] == null)
        {
            Debug.LogWarning($"[{name}] No hay configuración para la fase {fase}.");
            collidersActivos = new Collider[0];
            return;
        }

        ConfiguracionArmaFase config = armasPorFase[idx];

        if (config.objetosVisuales != null)
            foreach (var obj in config.objetosVisuales)
                if (obj != null) obj.SetActive(true);

        // Guardar los colliders activos de esta fase
        collidersActivos = config.collidersAtaque ?? new Collider[0];

        // Siempre desactivados entre ataques
        desactivarCollider();
    }

    // ─── Override de colliders ────────────────────────────────────────────────
    // El sistema base usa un solo ColliderArma. El jefe puede tener varios
    // por fase — sobreescribimos para gestionar la lista completa.

    public override void activarCollider()
    {
        foreach (var col in collidersActivos)
            if (col != null) col.enabled = true;
    }

    public override void desactivarCollider()
    {
        foreach (var col in collidersActivos)
            if (col != null) col.enabled = false;
    }
}
