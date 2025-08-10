using UnityEngine;
using System.Collections;

public class DetectarJugador : MonoBehaviour
{
    //Este script se encargara de detectar al jugador en un cono que representa la visión del enemigo, sin embargo, si el jugador se encuentra demasiado cerca del enemigo este será detectado
    [Header("Ajustes de Detección de Jugador")]
    [SerializeField] private float anguloDeDeteccion = 60f; //cono de visión del enemigo
    [SerializeField] float radioDeDeteccion = 15f; //este valor debe ser el mismo del rango de patrulla del enemigo
    [SerializeField] private float radioDeDeteccionAutomatica = 10f;
    [SerializeField] private float rangoDeAtaque = 3f;
    [SerializeField] private float tiempoPorDeteccion = 1f;
    
    public Transform Player {get; private set;}
    Temporizador temporizadorDeDetectarJugador;
    
    IEstrategiaDeDeteccion estrategiaDeDeteccion;

    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        temporizadorDeDetectarJugador = new Temporizador(tiempoPorDeteccion);
        estrategiaDeDeteccion =
            new EstrategiaDeDeteccionCono(anguloDeDeteccion, radioDeDeteccion, radioDeDeteccionAutomatica);
    }

    void Update() => temporizadorDeDetectarJugador.Tick(Time.deltaTime);

    public bool SePuedeDetectarAlJugador()
    {
        return temporizadorDeDetectarJugador.EstaCorriendo || estrategiaDeDeteccion.Ejecutar(Player, transform, temporizadorDeDetectarJugador);
    }

    public bool SePuedeAtacarAlJugador()
    {
        var direccionAlJugador = Player.position - transform.position;
        return direccionAlJugador.magnitude <= rangoDeAtaque;
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        // Draw a spheres for the radii
        Gizmos.DrawWireSphere(transform.position, radioDeDeteccion);
        Gizmos.DrawWireSphere(transform.position, radioDeDeteccionAutomatica);

        // Calculate our cone directions
        Vector3 forwardConeDirection = Quaternion.Euler(0, radioDeDeteccion / 2, 0) * transform.forward * radioDeDeteccion;
        Vector3 backwardConeDirection = Quaternion.Euler(0, -radioDeDeteccion / 2, 0) * transform.forward * radioDeDeteccion;

        // Draw lines to represent the cone
        Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
        Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
    }
    
 
}