using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectarJugador))]
[RequireComponent(typeof(HealthComp))]
public class Enemigo : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;

    MaquinaDeEstados maquinaDeEstados;
    HealthComp vidaEnemigo;
    private GameObject Jugador;

    [Header("Utility AI")]
    public UtilityAI_Grupal utilityGrupal;
    public UtilityAI_Tactico utilityTactico;

    private bool atacando = false;
    private bool disponibleParaAtacar = true;

    [Header("Stats del Enemigo")]
    [SerializeField] private EnemyStats stats;

    [Header("Sistema de Combo")]
    private int ataqueActualEnCombo = 0;
    private bool estaEnCombo = false;
    private TipoAtaque tipoAtaqueActual = TipoAtaque.Ligero;

    [Header("Detección")]
    [SerializeField] public DetectarJugador detectarJugador;

    [Header("Parametros Para Estado Atacar")]
    [SerializeField] public Collider ColliderArma;
    [SerializeField] public float rangoDeAtaque = 3f;
    private EstadoAtacarJugador estadoAtacarActual;

    [Header("Parametros Estado Patrulla")]
    [SerializeField] private float tiempoDeEspera = 1.5f;
    [SerializeField] private float radioDePatrulla = 15f;

    [Header("Parametros Estado Seguir")]
    [SerializeField] public float velocidadEnEstadoSeguir = 4f;

    [Header("Parametros para Esquivar")]
    [SerializeField] public float distanciaEsquivar = 3f;
    [SerializeField] public float velocidadEsquivar = 10f;
     private int layerNormal; 
     private int layerInvulnerable;

    [Header("Parametros Estado Daño")]
    [SerializeField] public float duracionDanoRecibido = 1.10f;

    // Cache de estados
    private Dictionary<Type, IEstado> estadosCache = new Dictionary<Type, IEstado>();

    // Estados reactivos
    private EstadoRebirDano estadoRecibirDano;
    private EstadoMuerte estadoMuerte;
    private EstadoStun estadoStun;
    private EstadoDeBloqueo estadoBloqueo;
    private EstadoRomperGuardia estadoRompeGuardia;
    private EstadoDeEsquivar estadoEsquivar;

    // Propiedades públicas
    public Transform JugadorActual => detectarJugador.Player;
    public EnemyStats Stats => stats;
    public int AtaqueActualEnCombo => ataqueActualEnCombo;
    public bool EstaEnCombo => estaEnCombo;
    public TipoAtaque TipoAtaqueActual => tipoAtaqueActual;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;

    public void Awake()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.animator = this.GetComponentInChildren<Animator>();
        this.vidaEnemigo = GetComponent<HealthComp>();
        desactivarCollider();
        BuscarJugador();

        layerNormal = LayerMask.NameToLayer("Enemigo");
        layerInvulnerable = LayerMask.NameToLayer("EnemigoInvulnerable");
    }

    public void BuscarJugador()
    {
        this.Jugador = EnemyManager.instance.Jugador;
    }

    void Start()
    {
        maquinaDeEstados = new MaquinaDeEstados();

        if (EnemyManager.instance != null)
        {
            utilityGrupal = new UtilityAI_Grupal(this, EnemyManager.instance);
            utilityTactico = new UtilityAI_Tactico(this);
        }
        else
        {
            Debug.LogError($"[{name}] EnemyManager no encontrado!");
        }

        if (stats == null)
        {
            Debug.LogError($"[{name}] No tiene EnemyStats asignado!");
        }

        InicializarEstadosReactivos();
        ConfigurarTransicionesReactivas();

        // Iniciar en patrulla
        var estadoInicial = new EstadoPatrullaEnemigo(this, animator, agent, radioDePatrulla, tiempoDeEspera);
        estadosCache[typeof(EstadoPatrullaEnemigo)] = estadoInicial;
        maquinaDeEstados.SetEstado(estadoInicial);
    }

    private void InicializarEstadosReactivos()
    {
        estadoRecibirDano = new EstadoRebirDano(this, animator, agent, vidaEnemigo, duracionDanoRecibido);
        estadoMuerte = new EstadoMuerte(this, animator, agent, vidaEnemigo);
        estadoStun = new EstadoStun(this, animator, agent, vidaEnemigo, vidaEnemigo.DuracionStun);
        estadoBloqueo = new EstadoDeBloqueo(this, animator, agent, vidaEnemigo);
        estadoRompeGuardia = new EstadoRomperGuardia(this, animator, agent, vidaEnemigo);
        estadoEsquivar = new EstadoDeEsquivar(this, animator, agent, vidaEnemigo, distanciaEsquivar, velocidadEsquivar);
    }

    private void ConfigurarTransicionesReactivas()
    {
        // Muerte (máxima prioridad)
        DesdeCualquier(estadoMuerte, new FuncPredicate(() => vidaEnemigo.EstaMuerto));

        // Recibir daño
        DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => vidaEnemigo.EnemigoFueDanado()));

        // Guard break → Stun
        Desde(estadoBloqueo, estadoRompeGuardia, new FuncPredicate(() => vidaEnemigo.EnGuardBreak));
        Desde(estadoRompeGuardia, estadoStun, new FuncPredicate(() => estadoRompeGuardia.guardBreakFinalizado));
    }

    // Método principal de evaluación (llamado por EnemyManager)
    public void EvaluarComportamiento()
    {
        if (vidaEnemigo == null || vidaEnemigo.EstaMuerto) return;

        // No evaluar si está en estados críticos
        if (estaEnCombo || vidaEnemigo.EstaStuneado || vidaEnemigo.EnGuardBreak || vidaEnemigo.EstaSiendoDanado)
        {
            return;
        }

        // ¿Detecta al jugador?
        if (!detectarJugador.SePuedeDetectarAlJugador())
        {
            // NO detecta → Patrullar
            CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        // SÍ detecta → Decidir con Utility AI
        AccionGrupal accionGrupal = utilityGrupal.DecidirAccion();
        
        switch (accionGrupal)
        {
            case AccionGrupal.Atacar:
                // Verificar si puede atacar por rango
                if (EstaAtacando() && detectarJugador.SePuedeAtacarAlJugador())
                {
                    // Decidir tipo de ataque
                    TipoDecisionTactica tactica = utilityTactico.DecidirAccionTactica();

                    if (tactica == TipoDecisionTactica.AtaqueLigero)
                        tipoAtaqueActual = TipoAtaque.Ligero;
                    else if (tactica == TipoDecisionTactica.AtaqueFuerte)
                        tipoAtaqueActual = TipoAtaque.Fuerte;

                    CambiarAEstado<EstadoAtacarJugador>();
                }
                else
                {
                    // Fuera de rango → Perseguir
                    CambiarAEstado<EstadoSeguirJugador>();
                }
                break;

            case AccionGrupal.Rodear:
                atacando = false;
                disponibleParaAtacar = true;
                CambiarAEstado<EstadoRodearJugador>();
                break;
            case AccionGrupal.Flanquear:
            case AccionGrupal.Retirarse:
                atacando = false;
                disponibleParaAtacar = true;
                CambiarAEstado<EstadoRodearJugador>();
                break;
        }
    }

    // Verificar si debe bloquear (llamado en Update)
    public void VerificarBloqueoYEsquive()
    {
        if (vidaEnemigo.EstaMuerto || vidaEnemigo.EnGuardBreak || vidaEnemigo.EstaStuneado) return;
        if (estaEnCombo) return; // No interrumpir combos

        if (!JugadorEstaAtacando()) return;

        // Decidir bloquear o esquivar
        TipoDecisionTactica decision = utilityTactico.DecidirAccionTactica();

        if (decision == TipoDecisionTactica.Bloquear && !vidaEnemigo.getBloqueando())
        {
            maquinaDeEstados.CambiarEstado(estadoBloqueo);
        }
        else if (decision == TipoDecisionTactica.Esquivar && !vidaEnemigo.EstaEsquivando)
        {
            maquinaDeEstados.CambiarEstado(estadoEsquivar);
        }
    }

    public void CambiarAEstado<T>() where T : IEstado
    {
        Type tipoEstado = typeof(T);

        if (!estadosCache.ContainsKey(tipoEstado))
        {
            IEstado nuevoEstado = CrearEstado<T>();
            if (nuevoEstado == null)
            {
                Debug.LogError($"[{name}] No se pudo crear {tipoEstado.Name}");
                return;
            }
            estadosCache[tipoEstado] = nuevoEstado;
        }

        maquinaDeEstados.CambiarEstado(estadosCache[tipoEstado]);
    }

    private IEstado CrearEstado<T>() where T : IEstado
    {
        Type tipo = typeof(T);

        if (tipo == typeof(EstadoPatrullaEnemigo))
            return new EstadoPatrullaEnemigo(this, animator, agent, radioDePatrulla, tiempoDeEspera);

        if (tipo == typeof(EstadoSeguirJugador))
            return new EstadoSeguirJugador(this, animator, agent, velocidadEnEstadoSeguir);

        if (tipo == typeof(EstadoAtacarJugador))
            return new EstadoAtacarJugador(this, animator, agent, rangoDeAtaque);

        if (tipo == typeof(EstadoRodearJugador))
            return new EstadoRodearJugador(this, animator, agent);

        return null;
    }

    // Métodos para EnemyManager
    public bool EstaDisponibleParaAtacar() => disponibleParaAtacar && !vidaEnemigo.EstaMuerto;
    public bool EstaAtacando() => atacando;
    public bool EstaMuerto() => vidaEnemigo.EstaMuerto;

    public void OrdenarAtacar()
    {
        // disponibleParaAtacar = false;
        atacando = true;
    }

    public void TerminarAtaque()
    {
        atacando = false;
        disponibleParaAtacar = true;
    }

    // Métodos auxiliares
    void Desde(IEstado estadoActual, IEstado estadoSiguiente, IPredicate condicion) =>
        maquinaDeEstados.AgregarTransicion(estadoActual, estadoSiguiente, condicion);

    void DesdeCualquier(IEstado estadoSiguiente, IPredicate condicion) =>
        maquinaDeEstados.AgregarTransicionGlobal(estadoSiguiente, condicion);

    void Update()
    {
        maquinaDeEstados.Update();
        vidaEnemigo.TickTimers(Time.deltaTime);

        // Verificar bloqueo/esquive constantemente
        VerificarBloqueoYEsquive();
    }

    void FixedUpdate()
    {
        maquinaDeEstados.FixedUpdate();
    }

    // Métodos de combo
    public void IniciarCombo(TipoAtaque tipoAtaque)
    {
        estaEnCombo = true;
        ataqueActualEnCombo = 0;
        tipoAtaqueActual = tipoAtaque;
    }

    public void SiguienteAtaqueEnCombo()
    {
        ataqueActualEnCombo++;
        int maxAtaques = tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.MaxAtaquesLigerosEnCombo
            : stats.MaxAtaquesFuertesEnCombo;

        if (ataqueActualEnCombo >= maxAtaques)
        {
            FinalizarCombo();
        }
    }

    public void FinalizarCombo()
    {
        estaEnCombo = false;
        ataqueActualEnCombo = 0;
    }

    public bool ComboCompletado()
    {
        int maxAtaques = tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.MaxAtaquesLigerosEnCombo
            : stats.MaxAtaquesFuertesEnCombo;

        return ataqueActualEnCombo >= maxAtaques;
    }

    public void RegistrarEstadoAtacar(EstadoAtacarJugador estado)
    {
        estadoAtacarActual = estado;
    }

    public void DesregistrarEstadoAtacar()
    {
        estadoAtacarActual = null;
    }

    public void OnAnimacionAtaqueCompletada()
    {
        if (estadoAtacarActual != null)
        {
            estadoAtacarActual.OnAnimacionAtaqueCompletada();
        }
    }

    public bool JugadorEstaAtacando()
    {
        if (Jugador == null || detectarJugador == null) return false;

        var combate = Jugador.GetComponent<ControladorCombate>();
        if (combate == null) return false;

        return combate.getAtacando();
    }

    public int ObtenerDanoActual()
    {
        return tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.DanoAtaqueLigero
            : stats.DanoAtaqueFuerte;
    }
    public void ActivarInvulnerabilidad()
    {
        gameObject.layer = layerInvulnerable;
    }

    public void DesactivarInvulnerabilidad()
    {
        gameObject.layer = layerNormal;
    }
    public void desactivarCollider()
    {
        ColliderArma.enabled = false;
    }

    public void activarCollider()
    {
        ColliderArma.enabled = true;
    }

    public HealthComp GetHealthComp()
    {
        return vidaEnemigo;
    }
}