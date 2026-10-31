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

    [Header("Stats del Enemigo")]
    [SerializeField] private EnemyStats stats;

    [Header("Deteccion")]
    public DetectarJugador detectarJugador;

    [Header("Colision de arma")]
    public Collider ColliderArma;

    // IA (la crea InicializarBase)
    public UtilityAI_Grupal  utilityGrupal;
    public UtilityAI_Tactico utilityTactico;

    private MaquinaDeEstados maquinaDeEstados;
    private HealthComp       vidaEnemigo;

    // Slot de ataque: lo concede EnemyManager (OrdenarAtacar) y se devuelve con TerminarAtaque.
    protected bool atacando;
    private   bool  disponibleParaAtacar = true;
    private   float disponibleDesde;
    private   float momentoOrdenAtacar;

    // Combo
    private int        ataqueActualEnCombo;
    private bool       estaEnCombo;
    private TipoAtaque tipoAtaqueActual = TipoAtaque.Ligero;

    private EstadoAtacarJugador estadoAtacarActual;

    // Layers de física
    private int layerNormal;
    private int layerInvulnerable;

    private readonly Dictionary<Type, IEstado> estadosCache = new Dictionary<Type, IEstado>();

    // Estados reactivos — protected para que subclases (distancia, jefe) puedan reusarlos
    protected EstadoRebirDano     estadoRecibirDano;
    protected EstadoMuerte        estadoMuerte;
    protected EstadoStun          estadoStun;
    protected EstadoDeBloqueo     estadoBloqueo;
    protected EstadoRomperGuardia estadoRompeGuardia;
    protected EstadoDeEsquivar    estadoEsquivar;

    // ─── Propiedades ──────────────────────────────────────────────────────────

    public Transform    JugadorActual       => detectarJugador != null ? detectarJugador.Player : null;
    public EnemyStats   Stats               => stats;
    public NavMeshAgent Agent               => agent;
    public Animator     Animator            => animator;
    public int          AtaqueActualEnCombo => ataqueActualEnCombo;
    public bool         EstaEnCombo         => estaEnCombo;
    public TipoAtaque   TipoAtaqueActual    => tipoAtaqueActual;

    // Parámetros leídos del SO (con valor de respaldo si aún no hay stats)
    public float RangoDeAtaque           => stats != null ? stats.RangoDeAtaque           : 3f;
    public float VelocidadEnEstadoSeguir => stats != null ? stats.VelocidadEnEstadoSeguir : 4f;
    public float DistanciaEsquivar       => stats != null ? stats.DistanciaEsquivar       : 3f;
    public float VelocidadEsquivar       => stats != null ? stats.VelocidadEsquivar       : 10f;
    public float DuracionDanoRecibido    => stats != null ? stats.DuracionDanoRecibido    : 2f;
    public float TiempoDeEspera          => stats != null ? stats.TiempoDeEspera          : 1.5f;
    public float RadioDePatrulla         => stats != null ? stats.RadioDePatrulla         : 15f;

    /// <summary>Nombre del estado actual de la FSM (para herramientas de diagnóstico).</summary>
    public string NombreEstadoActual
    {
        get
        {
            var estado = maquinaDeEstados?.ObtenerEstadoActual();
            return estado != null ? estado.GetType().Name : "-";
        }
    }

    protected HealthComp                VidaEnemigo  => vidaEnemigo;
    protected MaquinaDeEstados          FSM          => maquinaDeEstados;
    protected Dictionary<Type, IEstado> EstadosCache => estadosCache;

    // ─── Inicialización ───────────────────────────────────────────────────────

    public void Awake()
    {
        agent       = GetComponent<NavMeshAgent>();
        animator    = GetComponentInChildren<Animator>();
        vidaEnemigo = GetComponent<HealthComp>();

        desactivarCollider();

        layerNormal       = LayerMask.NameToLayer("Enemigo");
        layerInvulnerable = LayerMask.NameToLayer("EnemigoInvulnerable");
    }

    // protected virtual: el jefe controla su propia inicialización.
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

    // Separado de Start para que el jefe llame solo lo que necesita.
    protected void InicializarBase()
    {
        detectarJugador.Inicializar(stats);
        vidaEnemigo.Inicializar(stats);

        maquinaDeEstados = new MaquinaDeEstados();

        if (EnemyManager.instance == null)
        {
            Debug.LogError($"[{name}] EnemyManager no encontrado.");
            return;
        }

        EnemyManager.instance.RegistrarEnemigo(this);
        utilityGrupal  = new UtilityAI_Grupal(this, EnemyManager.instance);
        utilityTactico = new UtilityAI_Tactico(this);
    }

    protected virtual void InicializarEstadosReactivos()
    {
        estadoRecibirDano  = new EstadoRebirDano(this, animator, agent, vidaEnemigo, DuracionDanoRecibido);
        estadoMuerte       = new EstadoMuerte(this, animator, agent, vidaEnemigo);
        estadoStun         = new EstadoStun(this, animator, agent, vidaEnemigo, vidaEnemigo.DuracionStun);
        estadoBloqueo      = new EstadoDeBloqueo(this, animator, agent, vidaEnemigo);
        estadoRompeGuardia = new EstadoRomperGuardia(this, animator, agent, vidaEnemigo);
        estadoEsquivar     = new EstadoDeEsquivar(this, animator, agent, vidaEnemigo, DistanciaEsquivar, VelocidadEsquivar);

        estadosCache[typeof(EstadoRebirDano)]     = estadoRecibirDano;
        estadosCache[typeof(EstadoMuerte)]        = estadoMuerte;
        estadosCache[typeof(EstadoStun)]          = estadoStun;
        estadosCache[typeof(EstadoDeBloqueo)]     = estadoBloqueo;
        estadosCache[typeof(EstadoRomperGuardia)] = estadoRompeGuardia;
        estadosCache[typeof(EstadoDeEsquivar)]    = estadoEsquivar;
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

    // ─── Máquina de estados ───────────────────────────────────────────────────

    private void Update()
    {
        maquinaDeEstados.Update();
        vidaEnemigo.TickTimers(Time.deltaTime);
    }

    private void FixedUpdate() => maquinaDeEstados.FixedUpdate();

    protected void Desde(IEstado de, IEstado a, IPredicate condicion) =>
        maquinaDeEstados.AgregarTransicion(de, a, condicion);

    protected void DesdeCualquier(IEstado a, IPredicate condicion) =>
        maquinaDeEstados.AgregarTransicionGlobal(a, condicion);

    protected void RegistrarEstadoEnCache(Type tipo, IEstado estado) =>
        estadosCache[tipo] = estado;

    public void CambiarAEstado<T>() where T : IEstado
    {
        Type tipo = typeof(T);

        if (maquinaDeEstados == null)
        {
            Debug.LogError($"[{name}] La FSM no está inicializada al pedir {tipo.Name}.");
            return;
        }

        // Si el cache tiene la entrada pero con valor null, se recrea en vez de pasar null a la FSM.
        if (!estadosCache.TryGetValue(tipo, out IEstado estado) || estado == null)
        {
            estado = CrearEstado<T>();
            if (estado == null)
            {
                Debug.LogError($"[{name}] No se pudo crear estado: {tipo.Name}");
                return;
            }
            estadosCache[tipo] = estado;
        }

        maquinaDeEstados.CambiarEstado(estado);
    }

    /// <summary>
    /// Punto único de salida de los estados reactivos (stun, bloqueo, daño...): devuelve al enemigo
    /// al comportamiento que le corresponde. Sin slot de ataque NO persigue: rodea.
    /// </summary>
    public virtual void VolverAComportamientoBase()
    {
        if (!detectarJugador.SePuedeDetectarAlJugador())
        {
            TerminarAtaque();
            CambiarAEstado<EstadoPatrullaEnemigo>();
        }
        else if (atacando)
        {
            CambiarAEstado<EstadoSeguirJugador>();
        }
        else
        {
            CambiarAEstado<EstadoRodearJugador>();
        }
    }

    /// <summary>
    /// Lo llama EnemyManager si la IA del enemigo lanzó una excepción: libera el slot e intenta volver a un
    /// estado seguro; si tampoco se puede, lo deja quieto (mejor parado que persiguiendo al jugador sin control).
    /// </summary>
    public void RecuperarDeError()
    {
        TerminarAtaque();
        FinalizarCombo();

        try
        {
            VolverAComportamientoBase();
        }
        catch (Exception)
        {
            if (agent != null) agent.isStopped = true;
        }
    }

    /// <summary>Crea bajo demanda los estados de movimiento/ataque (se cachean en CambiarAEstado).</summary>
    protected virtual IEstado CrearEstado<T>() where T : IEstado
    {
        Type tipo = typeof(T);

        if (tipo == typeof(EstadoPatrullaEnemigo)) return new EstadoPatrullaEnemigo(this, animator, agent, RadioDePatrulla, TiempoDeEspera);
        if (tipo == typeof(EstadoSeguirJugador))   return new EstadoSeguirJugador(this, animator, agent, VelocidadEnEstadoSeguir);
        if (tipo == typeof(EstadoAtacarJugador))   return new EstadoAtacarJugador(this, animator, agent, RangoDeAtaque);
        if (tipo == typeof(EstadoRodearJugador))   return new EstadoRodearJugador(this, animator, agent);
        return null;
    }

    // ─── Comportamiento (lo invoca EnemyManager) ──────────────────────────────

    public virtual void EvaluarComportamiento()
    {
        if (vidaEnemigo == null || vidaEnemigo.EstaMuerto) return;
        if (EstaOcupado()) return;

        if (!detectarJugador.SePuedeDetectarAlJugador())
        {
            TerminarAtaque();
            CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        if (utilityGrupal.DecidirAccion() == AccionGrupal.Atacar)
        {
            EjecutarAccionAtacar();
            return;
        }

        // Rodear, retirarse, etc.: devuelve el slot de ataque y rodea.
        TerminarAtaque();
        CambiarAEstado<EstadoRodearJugador>();
    }

    private void EjecutarAccionAtacar()
    {
        if (!atacando)
        {
            CambiarAEstado<EstadoRodearJugador>();
            return;
        }

        if (!detectarJugador.SePuedeAtacarAlJugador(RangoDeAtaque))
        {
            CambiarAEstado<EstadoSeguirJugador>();
            return;
        }

        tipoAtaqueActual = utilityTactico.DecidirAccionTactica() == TipoDecisionTactica.AtaqueFuerte
            ? TipoAtaque.Fuerte
            : TipoAtaque.Ligero;

        CambiarAEstado<EstadoAtacarJugador>();
    }

    /// <summary>
    /// True si el enemigo está en medio de algo que no se debe interrumpir.
    /// Solo cuentan flags que duran lo mismo que su estado de la FSM. EnGuardBreak NO está aquí:
    /// su temporizador sigue corriendo cuando el stun ya terminó, y dejaba al enemigo sin evaluar
    /// su IA (persiguiendo al jugador) durante esa cola. Durante la guardia rota ya lo cubre EstaStuneado.
    /// </summary>
    private bool EstaOcupado() =>
        estaEnCombo
        || vidaEnemigo.EstaStuneado
        || vidaEnemigo.EstaSiendoDanado
        || vidaEnemigo.getBloqueando();

    public virtual void VerificarBloqueoYEsquive()
    {
        if (vidaEnemigo.EstaMuerto || vidaEnemigo.EnGuardBreak || vidaEnemigo.EstaStuneado) return;
        if (!JugadorEstaAtacando()) return;

        TipoDecisionTactica decision = utilityTactico.DecidirAccionTactica();

        if (decision == TipoDecisionTactica.Bloquear && !vidaEnemigo.getBloqueando())
            maquinaDeEstados.CambiarEstado(estadoBloqueo);
        else if (decision == TipoDecisionTactica.Esquivar && !vidaEnemigo.EstaEsquivando)
            maquinaDeEstados.CambiarEstado(estadoEsquivar);
    }

    // ─── Cambio de stats en runtime (fases del jefe) ──────────────────────────

    /// <summary>Cambia los stats activos y limpia del cache los estados que dependen de ellos.</summary>
    protected void ActualizarStats(EnemyStats nuevosStats)
    {
        stats = nuevosStats;
        LimpiarCacheDeEstadosDeMovimiento();
    }

    /// <summary>Los estados reactivos (muerte, daño, stun) no dependen de stats que cambien entre fases: se conservan.</summary>
    public void LimpiarCacheDeEstadosDeMovimiento()
    {
        estadosCache.Remove(typeof(EstadoAtacarJugador));
        estadosCache.Remove(typeof(EstadoSeguirJugador));
        estadosCache.Remove(typeof(EstadoDeEsquivar));
        estadosCache.Remove(typeof(EstadoRodearJugador));
        estadosCache.Remove(typeof(EstadoPatrullaEnemigo));
    }

    // ─── Slot de ataque (API para EnemyManager) ───────────────────────────────

    public bool EstaDisponibleParaAtacar() =>
        disponibleParaAtacar && Time.time >= disponibleDesde && !vidaEnemigo.EstaMuerto;

    public bool EstaAtacando() => atacando;
    public bool EstaMuerto()   => vidaEnemigo.EstaMuerto;

    /// <summary>Segundos desde que el manager le concedió el slot (0 si no tiene).</summary>
    public float TiempoConSlot => atacando ? Time.time - momentoOrdenAtacar : 0f;

    /// <summary>
    /// False en enemigos que gestionan su propio ritmo de ataque (el jefe puede sobreescribirlo):
    /// el manager no les quitará el slot por inactividad ni por cercanía.
    /// </summary>
    public virtual bool ParticipaEnIntercambioDeSlots => true;

    public void OrdenarAtacar()
    {
        if (!atacando) momentoOrdenAtacar = Time.time;
        atacando = true;
    }

    public void TerminarAtaque()
    {
        atacando             = false;
        disponibleParaAtacar = true;
    }

    /// <summary>Impide que vuelva a recibir slot durante unos segundos (evita reasignaciones inmediatas).</summary>
    public void EnfriarAtaque(float segundos) => disponibleDesde = Time.time + segundos;

    // ─── Combos ───────────────────────────────────────────────────────────────

    public void IniciarCombo(TipoAtaque tipo)
    {
        estaEnCombo         = true;
        ataqueActualEnCombo = 0;
        tipoAtaqueActual    = tipo;
    }

    // No finaliza el combo aquí: EstadoAtacarJugador lo hace al terminar la última animación.
    public void SiguienteAtaqueEnCombo() => ataqueActualEnCombo++;

    public void FinalizarCombo()
    {
        estaEnCombo         = false;
        ataqueActualEnCombo = 0;
    }

    public bool ComboCompletado() => ataqueActualEnCombo >= ObtenerMaxAtaquesCombo();

    private int ObtenerMaxAtaquesCombo() =>
        tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.MaxAtaquesLigerosEnCombo
            : stats.MaxAtaquesFuertesEnCombo;

    // ─── Callbacks de animación ───────────────────────────────────────────────

    public void RegistrarEstadoAtacar(EstadoAtacarJugador e) => estadoAtacarActual = e;
    public void DesregistrarEstadoAtacar()                   => estadoAtacarActual = null;
    public void OnAnimacionAtaqueCompletada()                => estadoAtacarActual?.OnAnimacionAtaqueCompletada();

    // ─── Consultas ────────────────────────────────────────────────────────────

    public bool JugadorEstaAtacando()
    {
        var manager = EnemyManager.instance;
        var combate = manager != null ? manager.CombateJugador : null;
        return combate != null && combate.getAtacando();
    }

    public int ObtenerDanoActual() =>
        tipoAtaqueActual == TipoAtaque.Ligero ? stats.DanoAtaqueLigero : stats.DanoAtaqueFuerte;

    public HealthComp GetHealthComp() => vidaEnemigo;

    // ─── Física ───────────────────────────────────────────────────────────────

    public void ActivarInvulnerabilidad()    => gameObject.layer = layerInvulnerable;
    public void DesactivarInvulnerabilidad() => gameObject.layer = layerNormal;

    public virtual void desactivarCollider() { if (ColliderArma) ColliderArma.enabled = false; }
    public virtual void activarCollider()    { if (ColliderArma) ColliderArma.enabled = true; }
}