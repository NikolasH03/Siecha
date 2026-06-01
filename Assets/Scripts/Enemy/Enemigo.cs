using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectarJugador))]
[RequireComponent(typeof(HealthComp))]
public class Enemigo : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private MaquinaDeEstados maquinaDeEstados;
    private HealthComp vidaEnemigo;

    [Header("Utility AI")]
    public UtilityAI_Grupal utilityGrupal;
    public UtilityAI_Tactico utilityTactico;

    private bool atacando           = false;
    private bool disponibleParaAtacar = true;

    [Header("Stats del Enemigo")]
    [SerializeField] private EnemyStats stats;

    [Header("Sistema de Combo")]
    private int        ataqueActualEnCombo = 0;
    private bool       estaEnCombo         = false;
    private TipoAtaque tipoAtaqueActual    = TipoAtaque.Ligero;

    [Header("Deteccion")]
    [SerializeField] public DetectarJugador detectarJugador;

    [Header("Colision de arma")]
    [SerializeField] public Collider ColliderArma;

    private EstadoAtacarJugador estadoAtacarActual;

    // Layers de fisica
    private int layerNormal;
    private int layerInvulnerable;

    // Cache de estados
    private Dictionary<Type, IEstado> estadosCache = new Dictionary<Type, IEstado>();

    // Estados reactivos — protected para que BossEnemigo pueda reusarlos
    protected EstadoRebirDano     estadoRecibirDano;
    protected EstadoMuerte        estadoMuerte;
    protected EstadoStun          estadoStun;
    protected EstadoDeBloqueo     estadoBloqueo;
    protected EstadoRomperGuardia estadoRompeGuardia;
    protected EstadoDeEsquivar    estadoEsquivar;

    // ─── Propiedades públicas ─────────────────────────────────────────────────

    public Transform    JugadorActual       => detectarJugador?.Player;
    public EnemyStats   Stats               => stats;
    public int          AtaqueActualEnCombo => ataqueActualEnCombo;
    public bool         EstaEnCombo         => estaEnCombo;
    public TipoAtaque   TipoAtaqueActual    => tipoAtaqueActual;
    public NavMeshAgent Agent               => agent;
    public Animator     Animator            => animator;

    // Parámetros leídos del SO
    public float RangoDeAtaque           => stats != null ? stats.RangoDeAtaque           : 3f;
    public float VelocidadEnEstadoSeguir => stats != null ? stats.VelocidadEnEstadoSeguir : 4f;
    public float DistanciaEsquivar       => stats != null ? stats.DistanciaEsquivar       : 3f;
    public float VelocidadEsquivar       => stats != null ? stats.VelocidadEsquivar       : 10f;
    public float DuracionDanoRecibido    => stats != null ? stats.DuracionDanoRecibido    : 1.1f;
    public float TiempoDeEspera          => stats != null ? stats.TiempoDeEspera          : 1.5f;
    public float RadioDePatrulla         => stats != null ? stats.RadioDePatrulla          : 15f;

    // Acceso protegido para subclases
    protected HealthComp                  VidaEnemigo  => vidaEnemigo;
    protected MaquinaDeEstados            FSM          => maquinaDeEstados;
    protected Dictionary<Type, IEstado>   EstadosCache => estadosCache;

    // ─── Awake ────────────────────────────────────────────────────────────────

    public void Awake()
    {
        agent       = GetComponent<NavMeshAgent>();
        animator    = GetComponentInChildren<Animator>();
        vidaEnemigo = GetComponent<HealthComp>();

        desactivarCollider();

        layerNormal       = LayerMask.NameToLayer("Enemigo");
        layerInvulnerable = LayerMask.NameToLayer("EnemigoInvulnerable");
    }

    // ─── Start ─────────────────────────────────────────────────────────────── 
    // protected virtual para que BossEnemigo pueda controlar su propia init.

    protected virtual void Start()
    {
        if (stats == null)
        {
            Debug.LogError($"[{name}] No tiene EnemyStats asignado.");
            enabled = false;
            return;
        }

        InicializarBase();
        InicializarEstadosReactivos();
        ConfigurarTransicionesReactivas();
        InicializarEstadoInicial();
    }

    // Separado de Start para que BossEnemigo llame solo lo que necesita.
    protected void InicializarBase()
    {
        detectarJugador.Inicializar(stats);
        vidaEnemigo.Inicializar(stats);

        maquinaDeEstados = new MaquinaDeEstados();

        if (EnemyManager.instance != null)
        {
            utilityGrupal  = new UtilityAI_Grupal(this, EnemyManager.instance);
            utilityTactico = new UtilityAI_Tactico(this);
        }
        else
        {
            Debug.LogError($"[{name}] EnemyManager no encontrado.");
        }
    }

    protected virtual void InicializarEstadosReactivos()
    {
        estadoRecibirDano  = new EstadoRebirDano(this, animator, agent, vidaEnemigo, DuracionDanoRecibido);
        estadoMuerte       = new EstadoMuerte(this, animator, agent, vidaEnemigo);
        estadoStun         = new EstadoStun(this, animator, agent, vidaEnemigo, vidaEnemigo.DuracionStun);
        estadoBloqueo      = new EstadoDeBloqueo(this, animator, agent, vidaEnemigo);
        estadoRompeGuardia = new EstadoRomperGuardia(this, animator, agent, vidaEnemigo);
        estadoEsquivar     = new EstadoDeEsquivar(this, animator, agent, vidaEnemigo, DistanciaEsquivar, VelocidadEsquivar);

        estadosCache[typeof(EstadoRebirDano)]      = estadoRecibirDano;
        estadosCache[typeof(EstadoMuerte)]         = estadoMuerte;
        estadosCache[typeof(EstadoStun)]           = estadoStun;
        estadosCache[typeof(EstadoDeBloqueo)]      = estadoBloqueo;
        estadosCache[typeof(EstadoRomperGuardia)]  = estadoRompeGuardia;
        estadosCache[typeof(EstadoDeEsquivar)]     = estadoEsquivar;
    }

    protected virtual void ConfigurarTransicionesReactivas()
    {
        DesdeCualquier(estadoMuerte,      new FuncPredicate(() => vidaEnemigo.EstaMuerto));
        DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => vidaEnemigo.EnemigoFueDanado()));

        Desde(estadoBloqueo,      estadoRompeGuardia, new FuncPredicate(() => vidaEnemigo.EnGuardBreak));
        Desde(estadoRompeGuardia, estadoStun,         new FuncPredicate(() => estadoRompeGuardia.guardBreakFinalizado));
    }

    protected virtual void InicializarEstadoInicial()
    {
        var estadoInicial = new EstadoPatrullaEnemigo(this, animator, agent, RadioDePatrulla, TiempoDeEspera);
        estadosCache[typeof(EstadoPatrullaEnemigo)] = estadoInicial;
        maquinaDeEstados.SetEstado(estadoInicial);
    }

    // ─── Helpers de transición — protected para subclases ────────────────────

    protected void Desde(IEstado de, IEstado a, IPredicate c) =>
        maquinaDeEstados.AgregarTransicion(de, a, c);

    protected void DesdeCualquier(IEstado a, IPredicate c) =>
        maquinaDeEstados.AgregarTransicionGlobal(a, c);

    protected void RegistrarEstadoEnCache(Type tipo, IEstado estado) =>
        estadosCache[tipo] = estado;

    // ─── Loop ─────────────────────────────────────────────────────────────────

    void Update()
    {
        maquinaDeEstados.Update();
        vidaEnemigo.TickTimers(Time.deltaTime);
    }

    void FixedUpdate() => maquinaDeEstados.FixedUpdate();

    // ─── Comportamiento (llamado por EnemyManager) ────────────────────────────

    public virtual void EvaluarComportamiento()
    {
        if (vidaEnemigo == null || vidaEnemigo.EstaMuerto) return;
        if (estaEnCombo || vidaEnemigo.EstaStuneado || vidaEnemigo.EnGuardBreak || vidaEnemigo.EstaSiendoDanado) return;

        if (!detectarJugador.SePuedeDetectarAlJugador())
        {
            CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        AccionGrupal accion = utilityGrupal.DecidirAccion();

        switch (accion)
        {
            case AccionGrupal.Atacar:
                if (atacando && detectarJugador.SePuedeAtacarAlJugador(RangoDeAtaque))
                {
                    TipoDecisionTactica tactica = utilityTactico.DecidirAccionTactica();
                    tipoAtaqueActual = tactica == TipoDecisionTactica.AtaqueFuerte
                        ? TipoAtaque.Fuerte : TipoAtaque.Ligero;
                    CambiarAEstado<EstadoAtacarJugador>();
                }
                else if (atacando)
                    CambiarAEstado<EstadoSeguirJugador>();
                else
                    CambiarAEstado<EstadoRodearJugador>();
                break;

            case AccionGrupal.Flanquear:
            case AccionGrupal.Rodear:
            case AccionGrupal.Retirarse:
            case AccionGrupal.Defender:
                atacando = false;
                disponibleParaAtacar = true;
                CambiarAEstado<EstadoRodearJugador>();
                break;

            default:
                Debug.LogWarning($"[{name}] AccionGrupal no manejada: {accion}");
                CambiarAEstado<EstadoRodearJugador>();
                break;
        }
    }

    public void VerificarBloqueoYEsquive()
    {
        if (vidaEnemigo.EstaMuerto || vidaEnemigo.EnGuardBreak || vidaEnemigo.EstaStuneado) return;
        if (estaEnCombo) return;
        if (!JugadorEstaAtacando()) return;

        TipoDecisionTactica decision = utilityTactico.DecidirAccionTactica();

        if (decision == TipoDecisionTactica.Bloquear && !vidaEnemigo.getBloqueando())
            maquinaDeEstados.CambiarEstado(estadoBloqueo);
        else if (decision == TipoDecisionTactica.Esquivar && !vidaEnemigo.EstaEsquivando)
            maquinaDeEstados.CambiarEstado(estadoEsquivar);
    }

    // ─── Sistema de estados ───────────────────────────────────────────────────

    public void CambiarAEstado<T>() where T : IEstado
    {
        Type tipo = typeof(T);
        if (!estadosCache.ContainsKey(tipo))
        {
            IEstado nuevo = CrearEstado<T>();
            if (nuevo == null)
            {
                Debug.LogError($"[{name}] No se pudo crear estado: {tipo.Name}");
                return;
            }
            estadosCache[tipo] = nuevo;
        }
        maquinaDeEstados.CambiarEstado(estadosCache[tipo]);
    }

    protected virtual IEstado CrearEstado<T>() where T : IEstado
    {
        Type tipo = typeof(T);
        if (tipo == typeof(EstadoPatrullaEnemigo))
            return new EstadoPatrullaEnemigo(this, animator, agent, RadioDePatrulla, TiempoDeEspera);
        if (tipo == typeof(EstadoSeguirJugador))
            return new EstadoSeguirJugador(this, animator, agent, VelocidadEnEstadoSeguir);
        if (tipo == typeof(EstadoAtacarJugador))
            return new EstadoAtacarJugador(this, animator, agent, RangoDeAtaque);
        if (tipo == typeof(EstadoRodearJugador))
            return new EstadoRodearJugador(this, animator, agent);
        return null;
    }

    // ─── Cambio de stats en runtime (para fases del jefe) ────────────────────

    /// <summary>
    /// Actualiza los stats activos y limpia el cache de estados que usan
    /// parámetros de movimiento/ataque. Los estados reactivos se mantienen.
    /// </summary>
    protected void ActualizarStats(EnemyStats nuevosStats)
    {
        stats = nuevosStats;
        LimpiarCacheDeEstadosDeMovimiento();
    }

    /// <summary>
    /// Limpia del cache solo los estados que dependen de stats de movimiento.
    /// Los estados reactivos (muerte, daño, stun) no se tocan porque no
    /// dependen de stats que cambien entre fases.
    /// </summary>
    public void LimpiarCacheDeEstadosDeMovimiento()
    {
        estadosCache.Remove(typeof(EstadoAtacarJugador));
        estadosCache.Remove(typeof(EstadoSeguirJugador));
        estadosCache.Remove(typeof(EstadoDeEsquivar));
        estadosCache.Remove(typeof(EstadoRodearJugador));
        estadosCache.Remove(typeof(EstadoPatrullaEnemigo));
    }

    // ─── API para EnemyManager ────────────────────────────────────────────────

    public bool EstaDisponibleParaAtacar() => disponibleParaAtacar && !vidaEnemigo.EstaMuerto;
    public bool EstaAtacando()             => atacando;
    public bool EstaMuerto()               => vidaEnemigo.EstaMuerto;
    public void OrdenarAtacar()            => atacando = true;
    public void TerminarAtaque()           { atacando = false; disponibleParaAtacar = true; }
    public void BuscarJugador()            => detectarJugador?.BuscarJugador();

    // ─── Combos ───────────────────────────────────────────────────────────────

    public void IniciarCombo(TipoAtaque tipo)
    {
        estaEnCombo = true; ataqueActualEnCombo = 0; tipoAtaqueActual = tipo;
    }
    public void SiguienteAtaqueEnCombo()
    {
        ataqueActualEnCombo++;
        if (ataqueActualEnCombo >= ObtenerMaxAtaquesCombo()) FinalizarCombo();
    }
    public void FinalizarCombo()     { estaEnCombo = false; ataqueActualEnCombo = 0; }
    public bool ComboCompletado()    => ataqueActualEnCombo >= ObtenerMaxAtaquesCombo();
    private int ObtenerMaxAtaquesCombo() =>
        tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.MaxAtaquesLigerosEnCombo
            : stats.MaxAtaquesFuertesEnCombo;

    // ─── Callbacks de animacion ───────────────────────────────────────────────

    public void RegistrarEstadoAtacar(EstadoAtacarJugador e) => estadoAtacarActual = e;
    public void DesregistrarEstadoAtacar()                   => estadoAtacarActual = null;
    public void OnAnimacionAtaqueCompletada()                => estadoAtacarActual?.OnAnimacionAtaqueCompletada();

    // ─── Consultas ────────────────────────────────────────────────────────────

    public bool JugadorEstaAtacando()
    {
        if (JugadorActual == null) return false;
        var c = JugadorActual.GetComponent<ControladorCombate>();
        return c != null && c.getAtacando();
    }

    public int ObtenerDanoActual() =>
        tipoAtaqueActual == TipoAtaque.Ligero ? stats.DanoAtaqueLigero : stats.DanoAtaqueFuerte;

    // ─── Fisica ───────────────────────────────────────────────────────────────

    public void ActivarInvulnerabilidad()    => gameObject.layer = layerInvulnerable;
    public void DesactivarInvulnerabilidad() => gameObject.layer = layerNormal;

    public virtual void desactivarCollider() { if (ColliderArma) ColliderArma.enabled = false; }
    public virtual void activarCollider()    { if (ColliderArma) ColliderArma.enabled = true; }

    public HealthComp GetHealthComp() => vidaEnemigo;
}