using System;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectarJugador))]
[RequireComponent((typeof(HealthComp)))]

public class EnemigoADistancia : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    
    MaquinaDeEstados maquinaDeEstadosED;
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
    [SerializeField] public float rangoMin = 30f;
    [SerializeField] public float rangoMax = 80f;
    public GameObject flecha;
    public Transform puntoDeDisparo;
    
    [Header("Parametros para estado de Esquivar Ataques")]
    [SerializeField] public float distanciaEsquivar = 3f;
    [SerializeField] public float velocidadEsquivar = 10f;
    
    public void Awake()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.animator = this.GetComponentInChildren<Animator>();
        this.vidaEnemigo = GetComponent<HealthComp>();
        this.controladorDeCombate = GameObject.FindGameObjectWithTag("Player");
    }
    
    private void OnValidate()
    {
        Debug.Assert(this.agent != null, "Se Debe Asignar un Enemigo");
        Debug.Assert(this.animator != null, "Se Debe Asignar un Animator");
    }

    private void Start()
    {
        maquinaDeEstadosED = new MaquinaDeEstados();

        var estadoPatrulla = new EstadoPatrullaED(this, animator, agent, radioDePatrulla, tiempoDeEspera);
        
        var estadoSeguir = new EstadoSeguirJugadorED(this, animator, agent, detectarJugador.Player, velocidadEnEstadoSeguir);
        
        var estadoAtacar = new EstadoAtacarJugadorED(this, animator, agent, detectarJugador.Player, rangoMin, rangoMax, tiempoEntreAtaques);
        
        Desde(estadoPatrulla, estadoSeguir, new FuncPredicate(() => detectarJugador.SePuedeDetectarAlJugador()));
        Desde(estadoSeguir, estadoPatrulla, new FuncPredicate(() => !detectarJugador.SePuedeDetectarAlJugador()));
        // Entrar al estado de ataque
        Desde(estadoSeguir, estadoAtacar, new FuncPredicate(() => detectarJugador.SePuedeAtacarAlJugador()));
        // Salir del estado de ataque si ya no se puede atacar
        Desde(estadoAtacar, estadoSeguir, new FuncPredicate(() => !detectarJugador.SePuedeAtacarAlJugador()));
        Desde(estadoPatrulla, estadoAtacar, new FuncPredicate(() => detectarJugador.SePuedeAtacarAlJugador()));


        
        
        maquinaDeEstadosED.SetEstado(estadoPatrulla);
    }
    
    // Transición desde un estado especifico hacia otro
    void Desde(IEstado estadoActual, IEstado estadoSiguiente, IPredicate condicion) => maquinaDeEstadosED.AgregarTransicion(estadoActual, estadoSiguiente, condicion);
    
    // Transición desde caulquier estado a otro
    void DesdeCualquier(IEstado estadoSiguiente, IPredicate condicion) => maquinaDeEstadosED.AgregarTransicionGlobal(estadoSiguiente, condicion);

    private void Update()
    {
        maquinaDeEstadosED.Update();
    }

    private void FixedUpdate()
    {
        maquinaDeEstadosED.FixedUpdate();
    }

    public void Disparar()
    {
        Instantiate(flecha, puntoDeDisparo.position, Quaternion.LookRotation(detectarJugador.Player.position - puntoDeDisparo.position));
    }
}