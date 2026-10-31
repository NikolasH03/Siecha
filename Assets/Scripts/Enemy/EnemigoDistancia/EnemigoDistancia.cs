using UnityEngine;

/// <summary>
/// Enemigo especializado en combate a distancia. Hereda de Enemigo para reutilizar
/// estados reactivos (muerte, daño, stun), FSM, EnemyManager, HealthComp y DetectarJugador.
/// No bloquea: VerificarBloqueoYEsquive se sobreescribe sin la rama de bloqueo.
///
/// Flujo: Patrulla → [detecta jugador] → Rodear ↔ ApuntarYDisparar
///        (jugador muy cerca → Reposicionarse; slot concedido pero lejos → Acercarse)
/// </summary>
public class EnemigoDistancia : Enemigo
{
    private const float AlturaObjetivo = 1f; // apunta al centro de masa del jugador, no a sus pies

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

    private EstadoApuntarYDisparar    estadoApuntar;
    private EstadoReposicionarse      estadoReposicion;
    private EstadoAcercarseADistancia estadoAcercarse;
    private EstadoRodearDistancia     estadoRodear;
    private EstadoEsquivarDistancia   estadoEsquivarDistancia;

    // Evita reentrada si un Animation Event dispara una evaluación durante un CambiarEstado.
    private bool evaluandoComportamiento;

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>
    /// El orden importa: estadoRodear se crea antes porque estadoEsquivarDistancia depende de él.
    /// </summary>
    protected override void InicializarEstadosReactivos()
    {
        estadoRecibirDano = new EstadoRecibirDanoDistancia(this, Animator, Agent, VidaEnemigo, DuracionDanoRecibido);
        estadoMuerte      = new EstadoMuerte(this, Animator, Agent, VidaEnemigo);
        estadoStun        = new EstadoStun(this, Animator, Agent, VidaEnemigo, VidaEnemigo.DuracionStun);

        estadoRodear            = new EstadoRodearDistancia(this, Animator, Agent);
        estadoEsquivarDistancia = new EstadoEsquivarDistancia(
            this, Animator, Agent, VidaEnemigo, DistanciaEsquivar, VelocidadEsquivar, estadoRodear);
        estadoApuntar    = new EstadoApuntarYDisparar(this, Animator, Agent, this, tiempoApuntado, tiempoCooldownDisparo);
        estadoReposicion = new EstadoReposicionarse(this, Animator, Agent, rangoMinimoConfort, distanciaReposicion, velocidadHuida);
        estadoAcercarse  = new EstadoAcercarseADistancia(this, Animator, Agent, Stats.VelocidadEnEstadoSeguir, RangoDeAtaque);

        // El campo base apunta al esquive correcto para que el código heredado use la instancia adecuada.
        estadoEsquivar = estadoEsquivarDistancia;

        // Cache por el tipo CONCRETO: MaquinaDeEstados indexa por GetType(); si el tipo del cache
        // no coincide se crean nodos duplicados y se rompe el guard "mismo estado = no hacer nada".
        EstadosCache[typeof(EstadoRecibirDanoDistancia)] = estadoRecibirDano;
        EstadosCache[typeof(EstadoMuerte)]               = estadoMuerte;
        EstadosCache[typeof(EstadoStun)]                 = estadoStun;
        EstadosCache[typeof(EstadoRodearDistancia)]      = estadoRodear;
        EstadosCache[typeof(EstadoEsquivarDistancia)]    = estadoEsquivarDistancia;
        EstadosCache[typeof(EstadoApuntarYDisparar)]     = estadoApuntar;
        EstadosCache[typeof(EstadoReposicionarse)]       = estadoReposicion;
        EstadosCache[typeof(EstadoAcercarseADistancia)]  = estadoAcercarse;

        // InicializarBase() ya creó la versión melee; se reemplaza por la calibrada para arquero.
        utilityGrupal = new UtilityAI_GrupalDistancia(this, EnemyManager.instance);
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
    /// Decide por prioridades directas, sin pasar por utilityGrupal.DecidirAccion():
    /// esa función libera el slot poniendo atacando = false sin avisar al EnemyManager.
    /// El arquero gestiona su slot solo a través de EnemyManager.
    /// </summary>
    public override void EvaluarComportamiento()
    {
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

            // No interrumpir un disparo o reposicionamiento en curso.
            var estadoActual = FSM.ObtenerEstadoActual();
            if (estadoActual == estadoApuntar     && !estadoApuntar.DisparoCompletado)              return;
            if (estadoActual == estadoReposicion  && !estadoReposicion.ReposicionamientoCompletado) return;

            if (!detectarJugador.SePuedeDetectarAlJugador())
            {
                TerminarAtaque();
                CambiarAEstado<EstadoPatrullaEnemigo>();
                return;
            }

            if (!EstaAtacando())
                FSM.CambiarEstado(estadoRodear);
            else if (distancia <= RangoDeAtaque)
                FSM.CambiarEstado(estadoApuntar);
            else
                FSM.CambiarEstado(estadoAcercarse);
        }
        finally
        {
            evaluandoComportamiento = false;
        }
    }

    /// <summary>Sin rama de bloqueo: más seguro que depender de PuedeBloquear = false en el SO.</summary>
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

    /// <summary>
    /// Tras stun/daño el arquero rodea (o patrulla si perdió al jugador); la siguiente evaluación
    /// decide si apunta o se acerca. Nunca usa EstadoSeguirJugador, que es de melee.
    /// </summary>
    public override void VolverAComportamientoBase()
    {
        if (!detectarJugador.SePuedeDetectarAlJugador())
        {
            TerminarAtaque();
            CambiarAEstado<EstadoPatrullaEnemigo>();
            return;
        }

        FSM.CambiarEstado(estadoRodear);
    }

    // ─── API para los estados ─────────────────────────────────────────────────

    /// <summary>Los estados no acceden a FSM directamente (es protected).</summary>
    public void CambiarAEstadoRodear() => FSM.CambiarEstado(estadoRodear);

    public void FinalizarAtaqueDistancia()
    {
        if (EnemyManager.instance != null) EnemyManager.instance.LiberarEnemigo(this);
        else                               TerminarAtaque();
    }

    public void Disparar()
    {
        if (prefabProyectil == null || spawnProyectil == null)
        {
            Debug.LogError($"[{name}] Falta prefabProyectil o spawnProyectil.");
            return;
        }

        Transform jugador = JugadorActual;
        if (jugador == null) return;

        Vector3 dir = (jugador.position + Vector3.up * AlturaObjetivo - spawnProyectil.position).normalized;
        GameObject instancia = Instantiate(prefabProyectil, spawnProyectil.position, Quaternion.LookRotation(dir));

        var proyectil = instancia.GetComponent<ProyectilEnemigo>();
        if (proyectil != null) proyectil.Inicializar(danoProyectil);
        else Debug.LogError($"[{name}] El prefab proyectil no tiene componente ProyectilEnemigo.");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private float DistanciaAlJugador() =>
        JugadorActual == null ? float.MaxValue : Vector3.Distance(transform.position, JugadorActual.position);

    // Sin arma cuerpo a cuerpo.
    public override void desactivarCollider() { }
    public override void activarCollider()    { }
}