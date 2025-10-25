using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectarJugador))]
[RequireComponent((typeof(HealthComp)))]

public class Enemigo : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    
    MaquinaDeEstados maquinaDeEstados;
    HealthComp vidaEnemigo;
    public GameObject Jugador;

    private bool atacando = false;
    private bool disponibleParaAtacar = true;

    [Header("Stats del Enemigo")] 
    [SerializeField] private EnemyStats stats;

    [Header("Sistema de Combo")] 
    private int ataqueActualEnCombo = 0;
    private bool estaEnCombo = false;
    private TipoAtaque tipoAtaqueActual = TipoAtaque.Ligero;

    [Header("Parametros Para Estado Patrulla")]
    [SerializeField] public float tiempoDeEspera = 1.5f;
    [SerializeField] public float radioDePatrulla = 8f;
    
    [Header("Parametros Para Estado Seguir")]
    [SerializeField] public DetectarJugador detectarJugador;

    [SerializeField] public float velocidadEnEstadoSeguir = 4f;

    [Header("Parametros Para Estado Atacar")]
    [SerializeField] public Collider ColliderArma;
    [SerializeField] public float tiempoEntreAtaques = 1f;
    [SerializeField] public float rangoDeAtaque = 1f;
    Temporizador tempParaAtaques;
    private EstadoAtacarJugador estadoAtacarActual;

    [Header("Parametros Para Estado de Bloqueo")]
    [SerializeField] public float rangoDeBloqueo = 4f;

    [Header("Parametros para estado de Esquivar Ataques")] 
    [SerializeField] public float probabilidadDeEsquivar = 1f;
    [SerializeField] public float distanciaEsquivar = 3f;
    [SerializeField] public float velocidadEsquivar = 10f;
    private bool intentoEsquivar = false;
    
    [Header("Parametros Para Estado Recibir Daño")]
    [SerializeField] public float duracionDanoRecibido = 1.10f;
    
    [Header("Parametros Para Estado Stun")]
    [SerializeField] public float duracionStun = 5f;
    
    [Header("Parametros Para Estado De Muerte")]
    [SerializeField] public float tiempoDeDesaparicion = 2f;

    public Transform JugadorActual => detectarJugador.Player;
    public EnemyStats Stats => stats;
    public int AtaqueActualEnCombo => ataqueActualEnCombo;
    public bool EstaEnCombo => estaEnCombo;
    public TipoAtaque TipoAtaqueActual => tipoAtaqueActual;


    public void Awake()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.animator = this.GetComponentInChildren<Animator>();
        this.vidaEnemigo = GetComponent<HealthComp>();
        BuscarJugador();
        tempParaAtaques = new Temporizador(tiempoEntreAtaques);
    }
    public void BuscarJugador()
    {
        this.Jugador = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnValidate()
    {
        Debug.Assert(this.agent != null, "Se Debe Asignar un Enemigo");
        Debug.Assert(this.animator != null, "Se Debe Asignar un Animator");
    }
    void Start()
    {
        maquinaDeEstados = new MaquinaDeEstados();

        if (stats == null)
        {
            Debug.LogError($"Enemigo {gameObject.name} no tiene EnemyStats asignado!");
        }

        var estadoPatrulla = new EstadoPatrullaEnemigo(this, animator, agent, radioDePatrulla, tiempoDeEspera);
        var estadoSeguir = new EstadoSeguirJugador(this, animator, agent, velocidadEnEstadoSeguir);
        var estadoAtacar = new EstadoAtacarJugador(this, animator, agent, rangoDeAtaque);
        var estadoRecibirDano = new EstadoRebirDano(this, animator, vidaEnemigo, duracionDanoRecibido);
        var estadoMuerte = new EstadoMuerte(this, animator, vidaEnemigo, tiempoDeDesaparicion);
        var estadoBloqueo = new EstadoDeBloqueo(this, animator, agent, vidaEnemigo);
        //var estadoSecuenciaDeAtaques = new EstadoSecuenciaDeAtaques(this, animator, agent, detectarJugador.Player,
        //    secuenciaAtaques, tempParaSecuencia, delayEntreAtaques);
        var estadoEsquivarAtaques = new EstadoDeEsquivar(this, animator, agent, vidaEnemigo, distanciaEsquivar, velocidadEsquivar);
        var estadoRompeGuardia = new EstadoRomperGuardia(this, animator, agent, vidaEnemigo);
        var estadoRodear = new EstadoRodearJugador(this, animator, agent);
        var estadoStun = new EstadoStun(this, animator, agent, vidaEnemigo, duracionStun);


        // Transiciones entre estados de Patrulla, Persecución y Ataque
        Desde(estadoPatrulla, estadoSeguir, new FuncPredicate(() => detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoSeguir, estadoPatrulla, new FuncPredicate(() => !detectarJugador.SePuedeDetectarAlJugador()));

        // Transiciones en el estado atacar normal
        Desde(estadoSeguir, estadoAtacar, new FuncPredicate(() => detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoAtacar, estadoSeguir, new FuncPredicate(() => !detectarJugador.SePuedeAtacarAlJugador()));

        // Transiciones en el estado atacar en secuencia
        // Desde(estadoSeguir, estadoSecuenciaDeAtaques, new FuncPredicate(() =>
        //     detectarJugador.SePuedeAtacarAlJugador()));
        // Desde(estadoSecuenciaDeAtaques, estadoSeguir, new FuncPredicate(() =>
        //     !detectarJugador.SePuedeAtacarAlJugador()));

        // Entrar al estado de recibir daño desde cualquier otro estado 
        DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => vidaEnemigo.EnemigoFueDanado()));

        // Transiciones para salir del estado de daño a cualquier otro estado
        Desde(estadoRecibirDano, estadoPatrulla, new FuncPredicate(() =>
            estadoRecibirDano.TerminoTiempoDano &&
            !detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoRecibirDano, estadoSeguir, new FuncPredicate(() =>
            estadoRecibirDano.TerminoTiempoDano &&
            detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoRecibirDano, estadoAtacar, new FuncPredicate(() =>
            estadoRecibirDano.TerminoTiempoDano &&
            detectarJugador.SePuedeAtacarAlJugador()));

        // Entrar al estado de muerte desde cualquier otro estado
        DesdeCualquier(estadoMuerte, new FuncPredicate(() => vidaEnemigo.EnemigoHaMuerto()));

        // Entrar al estado de bloqueo desde cualquier otro estado
        DesdeCualquier(estadoBloqueo, new FuncPredicate(SePuedeBloquearAlJugador));

        // Transiciones para salir del estado de bloqueo
        Desde(estadoBloqueo, estadoAtacar, new FuncPredicate(() =>
            !JugadorEstaAtacando() && detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoBloqueo, estadoSeguir, new FuncPredicate(() =>
            !JugadorEstaAtacando() && detectarJugador.SePuedeDetectarAlJugador() && !detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoBloqueo, estadoPatrulla, new FuncPredicate(() =>
            !JugadorEstaAtacando() && !detectarJugador.SePuedeDetectarAlJugador()));

        // Estado de guardia rota
        Desde(estadoBloqueo, estadoRompeGuardia, new FuncPredicate(() => vidaEnemigo.EnGuardBreak));
        Desde(estadoRompeGuardia, estadoStun, new FuncPredicate(() => estadoRompeGuardia.guardBreakFinalizado));

        // Primero evalúa si puede bloquear (mayor prioridad)
        Desde(estadoStun, estadoBloqueo, new FuncPredicate(() =>
            estadoStun.stunFinalizado && SePuedeBloquearAlJugador()));

        // Luego las demás transiciones
        Desde(estadoStun, estadoAtacar, new FuncPredicate(() =>
            estadoStun.stunFinalizado &&
            !SePuedeBloquearAlJugador() && // ← NUEVO: Solo atacar si NO debe bloquear
            detectarJugador.SePuedeAtacarAlJugador()));

        Desde(estadoStun, estadoSeguir, new FuncPredicate(() =>
            estadoStun.stunFinalizado &&
            !SePuedeBloquearAlJugador() && // ← NUEVO
            detectarJugador.SePuedeDetectarAlJugador()));

        Desde(estadoStun, estadoPatrulla, new FuncPredicate(() =>
            estadoStun.stunFinalizado &&
            !detectarJugador.SePuedeDetectarAlJugador()));


        // Rodear Jugador
        Desde(estadoSeguir, estadoRodear, new FuncPredicate(() => !EstaAtacando() && detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoRodear, estadoAtacar, new FuncPredicate(() => atacando));
        Desde(estadoAtacar, estadoRodear, new FuncPredicate(() => !atacando && detectarJugador.SePuedeDetectarAlJugador()));

        // Esquivar
        DesdeCualquier(estadoEsquivarAtaques, new FuncPredicate(SePuedeEsquivarAlJugador));
        Desde(estadoEsquivarAtaques, estadoAtacar, new FuncPredicate(() => !JugadorEstaAtacando() && detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoEsquivarAtaques, estadoSeguir, new FuncPredicate(() => !JugadorEstaAtacando() && detectarJugador.SePuedeDetectarAlJugador() && !detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoEsquivarAtaques, estadoPatrulla, new FuncPredicate(() => !JugadorEstaAtacando() && !detectarJugador.SePuedeDetectarAlJugador()));

        maquinaDeEstados.SetEstado(estadoPatrulla);
    }

    //Métodos para el EnemyManager.cs
    public bool EstaDisponibleParaAtacar() => disponibleParaAtacar && !vidaEnemigo.EstaMuerto;
    public bool EstaAtacando() => atacando;
    public bool EstaMuerto() => vidaEnemigo != null && vidaEnemigo.EstaMuerto;
    
    public void OrdenarAtacar()
    {
        disponibleParaAtacar = false;
        atacando = true;
    }
    
    public void TerminarAtaque()
    {
        atacando = false;
        disponibleParaAtacar = true;
    }
    
    //Métodos Auxiliares
    
    // Transición desde un estado especifico hacia otro
    void Desde(IEstado estadoActual, IEstado estadoSiguiente, IPredicate condicion) => maquinaDeEstados.AgregarTransicion(estadoActual, estadoSiguiente, condicion);
    
    // Transición desde caulquier estado a otro
    void DesdeCualquier(IEstado estadoSiguiente, IPredicate condicion) => maquinaDeEstados.AgregarTransicionGlobal(estadoSiguiente, condicion);

    void Update()
    {
        maquinaDeEstados.Update();
        tempParaAtaques.Tick(Time.deltaTime);
        vidaEnemigo.TickTimers(Time.deltaTime);
    }

    void FixedUpdate()
    {
        maquinaDeEstados.FixedUpdate();
    }

    //public void Atacar()
    //{
    //    if (tempParaAtaques.EstaCorriendo) return;

    //    tempParaAtaques.Empezar();
    //    //logica para hacer daño
    //}
    public void IniciarCombo(TipoAtaque tipoAtaque)
    {
        estaEnCombo = true;
        ataqueActualEnCombo = 0;
        tipoAtaqueActual = tipoAtaque;
        Debug.Log($"Iniciando combo de tipo: {tipoAtaque}");
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
        Debug.Log("Combo finalizado");
    }

    public bool ComboCompletado()
    {
        int maxAtaques = tipoAtaqueActual == TipoAtaque.Ligero
            ? stats.MaxAtaquesLigerosEnCombo
            : stats.MaxAtaquesFuertesEnCombo;

        return ataqueActualEnCombo >= maxAtaques;
    }
    public void OnAtaqueCompletado()
    {
        Debug.Log("Animación de ataque completada");
        // Este método será llamado por los estados que lo necesiten
    }
    public void RegistrarEstadoAtacar(EstadoAtacarJugador estado)
    {
        estadoAtacarActual = estado;
    }

    public void DesregistrarEstadoAtacar()
    {
        estadoAtacarActual = null;
    }

    // ← NUEVO: Método llamado desde Animation Event
    public void OnAnimacionAtaqueCompletada()
    {
        Debug.Log("Animation Event: Ataque completado");

        // Notificar al estado si existe
        if (estadoAtacarActual != null)
        {
            estadoAtacarActual.OnAnimacionAtaqueCompletada();
        }
    }
    public bool JugadorEstaAtacando()
    {
        var ataquesDeJugador = Jugador.GetComponent<ControladorCombate>();
        if (ataquesDeJugador == null || detectarJugador == null) return false;
    
        float distancia = Vector3.Distance(transform.position, detectarJugador.Player.position);
        return ataquesDeJugador.getAtacando() && distancia <= rangoDeBloqueo;
    }
    
    // private bool intentoBloquear = false;

    public bool SePuedeBloquearAlJugador()
    {
        var ataquesDeJugador = Jugador.GetComponent<ControladorCombate>();
        if (ataquesDeJugador == null || detectarJugador == null) return false;

        float distancia = Vector3.Distance(transform.position, detectarJugador.Player.position);
        bool estaEnRango = distancia <= rangoDeBloqueo;
        bool jugadorAtacando = ataquesDeJugador.getAtacando();

        // Bloquear solo si:
        // 1. Está en rango
        // 2. El jugador ataca
        // 3. El enemigo ya recibió los golpes necesarios para bloquear
        // 4. No está en guard break
        return jugadorAtacando && estaEnRango && vidaEnemigo.DebeBloquear() && !vidaEnemigo.EnGuardBreak;
    }

    public bool SePuedeEsquivarAlJugador()
    {
        var ataquesDeJugador = Jugador.GetComponent<ControladorCombate>();
        if (ataquesDeJugador == null || detectarJugador == null) return false;

        float distancia = Vector3.Distance(transform.position, detectarJugador.Player.position);
        bool estaEnRango = distancia <= rangoDeBloqueo; // puedes usar otro rango distinto para el esquive
        bool jugadorAtacando = ataquesDeJugador.getAtacando();

        // Reinicia el intento cuando el jugador deja de atacar
        if (!jugadorAtacando) intentoEsquivar = false;

        // Si el jugador está atacando, todavía no intentamos esquivar,
        // y pasa la probabilidad → esquiva
        if (jugadorAtacando && estaEnRango && !intentoEsquivar)
        {
            intentoEsquivar = true;
            return Random.value < probabilidadDeEsquivar;
        }

        return false;
    }

    public int ObtenerDanoActual()
    {
        if(tipoAtaqueActual == TipoAtaque.Ligero)
        {
            return stats.DanoAtaqueLigero;
        }
        else
        {
            return stats.DanoAtaqueFuerte;
        }
    }
    public void desactivarCollider()
    {
        ColliderArma.enabled = false;
    }
    public void activarCollider()
    {
        ColliderArma.enabled = true;
    }

}