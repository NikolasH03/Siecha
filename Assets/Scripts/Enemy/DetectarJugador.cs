using UnityEngine;

public class DetectarJugador : MonoBehaviour
{
    private float anguloDeDeteccion;
    private float radioDeDeteccion;
    private float radioDeDeteccionAutomatica;
    private float tiempoPorDeteccion;

    private Temporizador temporizadorDeDeteccion;
    private IEstrategiaDeDeteccion estrategiaDeDeteccion;
    private bool inicializado;

    /// <summary>
    /// El jugador siempre se lee del EnemyManager (única fuente de verdad),
    /// así que si el manager cambia de jugador, todos los enemigos lo siguen.
    /// </summary>
    public Transform Player
    {
        get
        {
            var manager = EnemyManager.instance;
            return manager != null && manager.Jugador != null ? manager.Jugador.transform : null;
        }
    }

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>Llamado por Enemigo antes de entrar en su primer estado.</summary>
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

    private void Update()
    {
        if (!inicializado) return;
        temporizadorDeDeteccion.Tick(Time.deltaTime);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

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

    private void OnDrawGizmos()
    {
        // Antes de Inicializar() los valores son 0: se usan valores de respaldo para verlos en editor.
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