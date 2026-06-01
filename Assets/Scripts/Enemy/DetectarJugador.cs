using UnityEngine;
public class DetectarJugador : MonoBehaviour
{
    private float anguloDeDeteccion;
    private float radioDeDeteccion;
    private float radioDeDeteccionAutomatica;
    private float tiempoPorDeteccion;

    public Transform Player { get; private set; }

    private Temporizador temporizadorDeDeteccion;
    private IEstrategiaDeDeteccion estrategiaDeDeteccion;
    private bool inicializado = false;

    // ─── Inicialización ───────────────────────────────────────────────────────

    void Awake()
    {
        BuscarJugador();
    }

    /// <summary>
    /// Llamado por Enemigo.Start() con los valores del EnemyStats.
    /// Debe ejecutarse antes de que el enemigo entre en su primer estado.
    /// </summary>
    public void Inicializar(EnemyStats stats)
    {
        anguloDeDeteccion          = stats.AnguloDeDeteccion;
        radioDeDeteccion           = stats.RadioDeDeteccion;
        radioDeDeteccionAutomatica = stats.RadioDeDeteccionAutomatica;
        tiempoPorDeteccion         = stats.TiempoPorDeteccion;

        temporizadorDeDeteccion = new Temporizador(tiempoPorDeteccion);
        estrategiaDeDeteccion   = new EstrategiaDeDeteccionCono(
            anguloDeDeteccion,
            radioDeDeteccion,
            radioDeDeteccionAutomatica
        );

        inicializado = true;
    }

    void Update()
    {
        if (!inicializado) return;
        temporizadorDeDeteccion.Tick(Time.deltaTime);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void BuscarJugador()
    {
        if (EnemyManager.instance == null)
        {
            Debug.LogWarning($"[{name}] BuscarJugador: EnemyManager no existe todavía.");
            return;
        }

        if (EnemyManager.instance.Jugador == null)
        {
            Debug.LogWarning($"[{name}] BuscarJugador: EnemyManager.Jugador es null.");
            return;
        }

        Player = EnemyManager.instance.Jugador.transform;
    }

    public bool SePuedeDetectarAlJugador()
    {
        if (!inicializado || Player == null) return false;

        return temporizadorDeDeteccion.EstaCorriendo ||
               estrategiaDeDeteccion.Ejecutar(Player, transform, temporizadorDeDeteccion);
    }
    
    public bool SePuedeAtacarAlJugador(float rangoDeAtaque)
    {
        if (Player == null) return false;
        return Vector3.Distance(transform.position, Player.position) <= rangoDeAtaque;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        // En editor (antes de Inicializar) usamos valores de respaldo para visualizar.
        float angulo    = anguloDeDeteccion          > 0 ? anguloDeDeteccion          : 120f;
        float radio     = radioDeDeteccion           > 0 ? radioDeDeteccion           : 15f;
        float radioAuto = radioDeDeteccionAutomatica > 0 ? radioDeDeteccionAutomatica : 5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radio);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioAuto);

        float mitad = angulo / 2f;
        Vector3 der = Quaternion.Euler(0,  mitad, 0) * transform.forward * radio;
        Vector3 izq = Quaternion.Euler(0, -mitad, 0) * transform.forward * radio;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + der);
        Gizmos.DrawLine(transform.position, transform.position + izq);
    }
}