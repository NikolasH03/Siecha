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
    private GameObject controladorDeCombate;
    
    [Header("Parametros Para Estado Patrulla")]
    [SerializeField] public float tiempoDeEspera;
    [SerializeField] public float radioDePatrulla;
    
    [Header("Parametros Para Estado Seguir")]
    [SerializeField] public DetectarJugador detectarJugador;

    [SerializeField] public float velocidadEnEstadoSeguir = 4f;
    
    [Header("Parametros Para Estado Atacar")]
    [SerializeField] public float tiempoEntreAtaques = 1f;
    Temporizador tempParaAtaques;
    
    [Header("Parametros Para Estado de Bloqueo")]
    [SerializeField] public float rangoDeBloqueo = 4f;
    [SerializeField] public float probabilidadDeBloqueo = 0.5f;
    private Temporizador tempParaSecuencia;
    
    [Header("Parametros para estado de Esquivar Ataques")]
    [SerializeField] public float distanciaEsquivar = 3f;
    [SerializeField] public float velocidadEsquivar = 10f;
    
    [Header("Parametros para Secuencia De Ataques")]
    [SerializeField] private SecuenciaAtaques[] secuenciaAtaques;
    [SerializeField] public float delayEntreAtaques = 1f;
    
    [Header("Parametros Para Estado Recibir Daño")]
    [SerializeField] public float duracionDanoRecibido = 1.10f;
    
    [Header("Parametros Para Estado De Muerte")]
    [SerializeField] public float tiempoDeDesaparicion = 2f;
    
    
    
    public void Awake()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.animator = this.GetComponentInChildren<Animator>();
        this.vidaEnemigo = GetComponent<HealthComp>();
        this.controladorDeCombate = GameObject.FindGameObjectWithTag("Player");
        tempParaAtaques = new Temporizador(tiempoEntreAtaques);
    }

    private void OnValidate()
    {
        Debug.Assert(this.agent != null, "Se Debe Asignar un Enemigo");
        Debug.Assert(this.animator != null, "Se Debe Asignar un Animator");
    }

    void Start()
    {
        
        maquinaDeEstados = new MaquinaDeEstados();

        var estadoPatrulla = new EstadoPatrullaEnemigo(this, animator, agent, radioDePatrulla, tiempoDeEspera);
        
        var estadoSeguir = new EstadoSeguirJugador(this, animator, agent, detectarJugador.Player, velocidadEnEstadoSeguir);
        
        var estadoAtacar = new EstadoAtacarJugador(this, animator, agent, detectarJugador.Player);
        
        var estadoRecibirDano = new EstadoRebirDano(this, animator, vidaEnemigo, duracionDanoRecibido);

        var estadoMuerte = new EstadoMuerte(this, animator, vidaEnemigo, tiempoDeDesaparicion);

        var estadoBloqueo = new EstadoDeBloqueo(this, animator, agent, vidaEnemigo);

        var estadoSecuenciaDeAtaques = new EstadoSecuenciaDeAtaques(this, animator, agent, detectarJugador.Player,
            secuenciaAtaques, tempParaSecuencia, delayEntreAtaques);

        var estadoEsquivarAtaques =
            new EstadoDeEsquivar(this, animator, agent, vidaEnemigo, distanciaEsquivar, velocidadEsquivar);
        
        // Transiciones entre estados de Patrulla, Persecución y Ataque
        Desde(estadoPatrulla, estadoSeguir, new FuncPredicate(() => detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoSeguir, estadoPatrulla, new FuncPredicate(() => !detectarJugador.SePuedeDetectarAlJugador()));
        
        // Transiciones en el estado atacar normal
        // Desde(estadoSeguir, estadoAtacar, new FuncPredicate(() => detectarJugador.SePuedeAtacarAlJugador()));
        // Desde(estadoAtacar, estadoSeguir, new FuncPredicate(() => !detectarJugador.SePuedeAtacarAlJugador()));
        
        // Transiciones en el estado atacar en secuencia
        Desde(estadoSeguir, estadoSecuenciaDeAtaques, new FuncPredicate(() =>
            detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoSecuenciaDeAtaques, estadoSeguir, new FuncPredicate(() =>
            !detectarJugador.SePuedeAtacarAlJugador()));

        
        // // Entrar al estado de recibir daño desde cualquier otro estado 
        // DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => vidaEnemigo.EnimigoFueDanado()));
        //
        // // Transiciones para salir del estado de daño a cualquier otro estado
        // Desde(estadoRecibirDano, estadoPatrulla, new FuncPredicate(() => 
        //     estadoRecibirDano.TerminoTiempoDano && 
        //     !detectarJugador.SePuedeDetectarAlJugador()));
        // Desde(estadoRecibirDano, estadoSeguir, new FuncPredicate(() => 
        //     estadoRecibirDano.TerminoTiempoDano && 
        //     detectarJugador.SePuedeDetectarAlJugador()));
        //
        // Desde(estadoRecibirDano, estadoAtacar, new FuncPredicate(() => 
        //     estadoRecibirDano.TerminoTiempoDano && 
        //     detectarJugador.SePuedeAtacarAlJugador()));
        
        // Entrar al estado de muerte desde cualquier otro estado
        DesdeCualquier(estadoMuerte, new FuncPredicate(() => vidaEnemigo.EnemigoHaMuerto()));
        
        // // Entrar al estado de bloqueo desde cualquier otro estado (temporal)
        // DesdeCualquier(estadoBloqueo, new FuncPredicate(JugadorEstaAtacando));
        //
        // // Transiciones para salir del estado de bloqueo a cualquier otro estado
        // Desde(estadoBloqueo, estadoAtacar, new FuncPredicate(() =>
        //     !JugadorEstaAtacando() && detectarJugador.SePuedeAtacarAlJugador()));
        //
        // Desde(estadoBloqueo, estadoSeguir, new FuncPredicate(() =>
        //     !JugadorEstaAtacando() && detectarJugador.SePuedeDetectarAlJugador() && !detectarJugador.SePuedeAtacarAlJugador()));
        //
        // Desde(estadoBloqueo, estadoPatrulla, new FuncPredicate(() =>
        //     !JugadorEstaAtacando() && !detectarJugador.SePuedeDetectarAlJugador()));
        //
        
        // Entra al estado de Esquivar ataques desde cualquier estado
        DesdeCualquier(estadoEsquivarAtaques, new FuncPredicate(JugadorEstaAtacando));
        
        Desde(estadoEsquivarAtaques, estadoSecuenciaDeAtaques, new FuncPredicate(() => !JugadorEstaAtacando() && detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoEsquivarAtaques, estadoSeguir, new FuncPredicate(() => !JugadorEstaAtacando() && detectarJugador.SePuedeDetectarAlJugador() && !detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoEsquivarAtaques, estadoPatrulla, new FuncPredicate(() => !JugadorEstaAtacando() && !detectarJugador.SePuedeDetectarAlJugador()));
        
        maquinaDeEstados.SetEstado(estadoPatrulla);
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
    }

    void FixedUpdate()
    {
        maquinaDeEstados.FixedUpdate();
    }

    public void Atacar()
    {
        if(tempParaAtaques.EstaCorriendo) return;
        
        tempParaAtaques.Empezar();
        //logica para hacer daño
    }

    public bool JugadorEstaAtacando()
    {
        var ataquesDeJugador = controladorDeCombate.GetComponent<ControladorCombate>();
        if (ataquesDeJugador == null || detectarJugador == null) return false;
    
        float distancia = Vector3.Distance(transform.position, detectarJugador.Player.position);
        return ataquesDeJugador.getAtacando() && distancia <= rangoDeBloqueo;
    }
}