using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Variante del enemigo base especializada en combate a distancia.
/// 
/// HEREDA DE Enemigo porque:
///   - Reutiliza todos los estados reactivos (muerte, daño, stun, esquivar)
///   - Reutiliza la FSM, EnemyManager, HealthComp y DetectarJugador
///   - Solo sobreescribe el comportamiento ofensivo y el estado inicial
/// 
/// NO USA bloqueo: estadoBloqueo nunca se inicializa, y VerificarBloqueoYEsquive
/// se sobreescribe para eliminar esa rama completamente.
/// 
/// FLUJO DE COMPORTAMIENTO:
///   Patrulla → [detecta jugador] → Rodear ↔ ApuntarYDisparar
///                                      ↑ jugador muy cerca
///                                      ↓ jugador lejos
///                                  Reposicionarse
/// </summary>
public class EnemigoDistancia : Enemigo
{
    // ─── Configuración exclusiva ──────────────────────────────────────────────

    [Header("Combate a Distancia")]
    [Tooltip("Prefab del proyectil. Debe tener ProyectilEnemigo y Rigidbody.")]
    [SerializeField] private GameObject prefabProyectil;

    [Tooltip("Transform desde donde sale el proyectil (hueso del arco/arma).")]
    [SerializeField] private Transform spawnProyectil;

    [Tooltip("Daño que causa cada proyectil.")]
    [SerializeField] private int danoProyectil = 10;

    [Tooltip("Segundos apuntando antes de disparar.")]
    [SerializeField] private float tiempoApuntado = 1.5f;

    [Tooltip("Segundos de cooldown después de disparar.")]
    [SerializeField] private float tiempoCooldownDisparo = 2f;

    [Tooltip("Si el jugador entra a menos de esta distancia, el enemigo se reposiciona.")]
    [SerializeField] private float rangoMinimoConfort = 5f;

    [Tooltip("Distancia objetivo al reposicionarse.")]
    [SerializeField] private float distanciaReposicion = 10f;

    [Tooltip("Velocidad de movimiento al huir.")]
    [SerializeField] private float velocidadHuida = 5f;

    // ─── Estados propios ──────────────────────────────────────────────────────

    private EstadoApuntarYDisparar    estadoApuntar;
    private EstadoReposicionarse      estadoReposicion;
    private EstadoAcercarseADistancia estadoAcercarse;
    private EstadoRodearDistancia     estadoRodear;
    private EstadoEsquivarDistancia   estadoEsquivarDistancia;

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>
    /// ORDEN DE INSTANCIACIÓN IMPORTA:
    /// estadoRodear → estadoEsquivarDistancia (depende de estadoRodear)
    /// Todo lo demás es independiente.
    /// </summary>
    protected override void InicializarEstadosReactivos()
    {
        // Estados reactivos heredados
        estadoRecibirDano = new EstadoRecibirDanoDistancia(this, Animator, Agent, VidaEnemigo, DuracionDanoRecibido);
        estadoMuerte      = new EstadoMuerte(this, Animator, Agent, VidaEnemigo);
        estadoStun        = new EstadoStun(this, Animator, Agent, VidaEnemigo, VidaEnemigo.DuracionStun);

        // Estados propios — rodear primero porque esquivar depende de él
        estadoRodear            = new EstadoRodearDistancia(this, Animator, Agent);
        estadoEsquivarDistancia = new EstadoEsquivarDistancia(
            this, Animator, Agent, VidaEnemigo,
            DistanciaEsquivar, VelocidadEsquivar,
            estadoRodear
        );
        estadoApuntar    = new EstadoApuntarYDisparar(
            this, Animator, Agent, this,
            tiempoApuntado, tiempoCooldownDisparo
        );
        estadoReposicion = new EstadoReposicionarse(
            this, Animator, Agent,
            rangoMinimoConfort, distanciaReposicion,
            velocidadHuida
        );
        estadoAcercarse  = new EstadoAcercarseADistancia(
            this, Animator, Agent,
            Stats.VelocidadEnEstadoSeguir, RangoDeAtaque
        );

        // Apuntar el campo base al esquive correcto para que
        // VerificarBloqueoYEsquive (sobreescrito) use la instancia correcta.
        estadoEsquivar = estadoEsquivarDistancia;

        // Registrar en cache — usar typeof del tipo concreto, no del base,
        // porque MaquinaDeEstados indexa por estado.GetType() y si el tipo
        // del cache no coincide crea nodos duplicados que rompen el guard
        // "si es el mismo estado no hacer nada" de CambiarEstado().
        EstadosCache[typeof(EstadoRecibirDanoDistancia)] = estadoRecibirDano;
        EstadosCache[typeof(EstadoMuerte)]              = estadoMuerte;
        EstadosCache[typeof(EstadoStun)]                = estadoStun;
        EstadosCache[typeof(EstadoRodearDistancia)]     = estadoRodear;
        EstadosCache[typeof(EstadoEsquivarDistancia)]   = estadoEsquivarDistancia;
        EstadosCache[typeof(EstadoApuntarYDisparar)]    = estadoApuntar;
        EstadosCache[typeof(EstadoReposicionarse)]      = estadoReposicion;
        EstadosCache[typeof(EstadoAcercarseADistancia)] = estadoAcercarse;

        // Reemplazar UtilityAI_Grupal melee por versión calibrada para arquero.
        // InicializarBase() ya instanció la versión melee; la sobreescribimos aquí.
        utilityGrupal = new UtilityAI_Grupal(this, EnemyManager.instance);
    }

    protected override void ConfigurarTransicionesReactivas()
    {
        DesdeCualquier(estadoMuerte,      new FuncPredicate(() => VidaEnemigo.EstaMuerto));
        DesdeCualquier(estadoRecibirDano, new FuncPredicate(() => VidaEnemigo.EnemigoFueDanado()));
    }

    protected override void InicializarEstadoInicial()
    {
        var estadoPatrulla = new EstadoPatrullaEnemigo(this, Animator, Agent, RadioDePatrulla, TiempoDeEspera);
        EstadosCache[typeof(EstadoPatrullaEnemigo)] = estadoPatrulla;
        FSM.SetEstado(estadoPatrulla);
    }

    // ─── Comportamiento ───────────────────────────────────────────────────────

    /// <summary>
    /// Decisión por prioridades directas, sin pasar por utilityGrupal.DecidirAccion().
    /// 
    /// POR QUÉ NO USAR utilityGrupal.DecidirAccion():
    /// Cuando esa función decide Rodear/Retirarse, hace atacando = false
    /// directamente sin llamar a EnemyManager.LiberarEnemigo(). Eso crea un
    /// estado inconsistente: EnemyManager cree que el arquero tiene slot pero
    /// atacando interno es false. El arquero gestiona su slot exclusivamente
    /// a través de EnemyManager.
    /// </summary>
    private bool evaluandoComportamiento = false;

    public override void EvaluarComportamiento()
    {
        // Guard contra recursión: si EvaluarComportamiento ya está en el stack
        // (por ejemplo disparado por un Animation Event durante un CambiarEstado),
        // salir inmediatamente. El AILoop lo llamará de nuevo en el próximo tick.
        if (evaluandoComportamiento) return;
        evaluandoComportamiento = true;

        try
        {
            if (VidaEnemigo == null || VidaEnemigo.EstaMuerto)            return;
            if (VidaEnemigo.EstaStuneado || VidaEnemigo.EstaSiendoDanado) return;

            float distancia = DistanciaAlJugador();

            if (distancia < rangoMinimoConfort)
            {
                FSM.CambiarEstado(estadoReposicion);
                return;
            }

            bool estaApuntando      = FSM.ObtenerEstadoActual() == estadoApuntar;
            bool estaReposicionando = FSM.ObtenerEstadoActual() == estadoReposicion;

            if (estaApuntando      && !estadoApuntar.DisparoCompletado)              return;
            if (estaReposicionando && !estadoReposicion.ReposicionamientoCompletado) return;

            if (!detectarJugador.SePuedeDetectarAlJugador())
            {
                CambiarAEstado<EstadoPatrullaEnemigo>();
                return;
            }

            if (EstaAtacando() && distancia <= RangoDeAtaque)
            {
                FSM.CambiarEstado(estadoApuntar);
                return;
            }

            if (EstaAtacando())
            {
                FSM.CambiarEstado(estadoAcercarse);
                return;
            }

            FSM.CambiarEstado(estadoRodear);
        }
        finally
        {
            evaluandoComportamiento = false;
        }
    }

    // ─── Sobreescritura de VerificarBloqueoYEsquive ───────────────────────────

    /// <summary>
    /// Elimina la rama de bloqueo completamente.
    /// Más seguro que confiar en PuedeBloquear = false en el SO.
    /// </summary>
    public override void VerificarBloqueoYEsquive()
    {
        if (VidaEnemigo.EstaMuerto || VidaEnemigo.EnGuardBreak || VidaEnemigo.EstaStuneado) return;
        if (!JugadorEstaAtacando()) return;

        if (utilityTactico.DecidirAccionTactica() == TipoDecisionTactica.Esquivar
            && !VidaEnemigo.EstaEsquivando)
        {
            FSM.CambiarEstado(estadoEsquivarDistancia);
        }
    }

    // ─── API para estados ─────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por EstadoEsquivarDistancia al terminar el esquive.
    /// Evita que el estado acceda a FSM directamente (es protected).
    /// </summary>
    public void CambiarAEstadoRodear() => FSM.CambiarEstado(estadoRodear);

    // ─── API de disparo ───────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por EstadoApuntarYDisparar en la fase de disparo.
    /// +Vector3.up apunta al centro de masa del jugador, no a sus pies.
    /// Ajusta el valor si el proyectil sale muy alto o bajo.
    /// </summary>
    public void Disparar()
    {
        if (prefabProyectil == null || spawnProyectil == null)
        {
            Debug.LogError($"[{name}] Falta prefabProyectil o spawnProyectil.");
            return;
        }

        Transform jugador = JugadorActual;
        if (jugador == null) return;

        Vector3    dir      = (jugador.position + Vector3.up * 1f - spawnProyectil.position).normalized;
        GameObject instancia = Instantiate(prefabProyectil, spawnProyectil.position, Quaternion.LookRotation(dir));

        var proyectil = instancia.GetComponent<ProyectilEnemigo>();
        if (proyectil != null)
            proyectil.Inicializar(danoProyectil);
        else
            Debug.LogError($"[{name}] El prefab proyectil no tiene componente ProyectilEnemigo.");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private float DistanciaAlJugador()
    {
        if (JugadorActual == null) return float.MaxValue;
        return Vector3.Distance(transform.position, JugadorActual.position);
    }

    public override void desactivarCollider() { }
    public override void activarCollider()    { }
}
