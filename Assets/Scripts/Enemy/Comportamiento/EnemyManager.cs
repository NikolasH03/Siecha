using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Coordina a todos los enemigos: les pide evaluar su comportamiento, reparte los
/// "slots" de ataque (cuántos pueden atacar a la vez) y expone al jugador.
/// Corre en coroutines con intervalos, no en Update().
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Configuracion de Combate")]
    [SerializeField] private int   maxEnemigosMeleeAtacandoSimultaneamente = 2;
    [SerializeField] private int   maxArquerosAtacandoSimultaneamente      = 4;
    [SerializeField] private float intervaloEvaluacionAI   = 0.1f;
    [SerializeField] private float intervaloGestionSlots   = 0.5f;
    [SerializeField] private float intervaloBloqueoEsquive = 0.05f;

    [Header("Rodeo")]
    [Tooltip("Separacion angular minima (grados) entre enemigos que rodean al jugador.")]
    [SerializeField] private float separacionAngularMinima = 45f;

    [Tooltip("Grados que avanza cada enemigo alrededor del jugador en cada actualizacion de su posicion (0 = no orbitan).")]
    [SerializeField] private float derivaOrbitalPorActualizacion = 10f;

    [Header("Reasignacion de slots")]
    [Tooltip("Un candidato le quita el slot a un atacante solo si esta al menos esta cantidad de metros mas cerca (evita cambios constantes).")]
    [SerializeField] private float margenCambioSlot = 1.5f;

    [Tooltip("Si un atacante lleva tanto tiempo con slot sin iniciar un combo, se le quita (protege contra enemigos atascados).")]
    [SerializeField] private float tiempoMaximoConSlot = 10f;

    [Tooltip("Segundos que un enemigo al que se le quito el slot no puede recibirlo de nuevo.")]
    [SerializeField] private float enfriamientoTrasLiberar = 2f;

    public List<Enemigo> todosLosEnemigos = new List<Enemigo>();
    public List<Enemigo> enemigosAtacando = new List<Enemigo>();

    public GameObject Jugador;
    public DetectorObjetivoJugador DetectorObjetivo { get; private set; }
    public ControladorCombate      CombateJugador   { get; private set; }

    private Coroutine aiLoopCoroutine;
    private Coroutine gestionSlotsCoroutine;
    private Coroutine bloqueoEsquiveCoroutine;

    private const float IntervaloLogFalla = 3f;
    private readonly Dictionary<Enemigo, float> ultimoLogDeFalla = new Dictionary<Enemigo, float>();

    // Buffers reutilizables para no generar basura en cada tick.
    private readonly List<Enemigo> snapshot           = new List<Enemigo>();
    private readonly List<Enemigo> candidatosMelee    = new List<Enemigo>();
    private readonly List<Enemigo> candidatosDistancia = new List<Enemigo>();

    // ─── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        AsignarJugador(GameObject.FindGameObjectWithTag("Player"));
        StartCoroutine(InicializarDespuesDeUnFrame());
    }

    /// <summary>
    /// Espera un frame para que todos los enemigos de la escena hayan corrido su Awake/Start
    /// antes de registrarlos y arrancar los loops.
    /// </summary>
    private IEnumerator InicializarDespuesDeUnFrame()
    {
        yield return null;

        todosLosEnemigos.Clear();
        enemigosAtacando.Clear();

        foreach (var enemigo in FindObjectsOfType<Enemigo>())
            RegistrarEnemigo(enemigo);

        Reiniciar(ref aiLoopCoroutine,         AILoop());
        Reiniciar(ref gestionSlotsCoroutine,   GestionarSlotsLoop());
        Reiniciar(ref bloqueoEsquiveCoroutine, BloqueoEsquiveLoop());
    }

    private void Reiniciar(ref Coroutine actual, IEnumerator rutina)
    {
        if (actual != null) StopCoroutine(actual);
        actual = StartCoroutine(rutina);
    }

    private void AsignarJugador(GameObject jugador)
    {
        if (jugador == null)
        {
            Debug.LogError("[EnemyManager] No se encontró objeto con tag 'Player'.");
            return;
        }

        Jugador          = jugador;
        DetectorObjetivo = jugador.GetComponent<DetectorObjetivoJugador>();
        CombateJugador   = jugador.GetComponent<ControladorCombate>();

        if (DetectorObjetivo == null)
            Debug.LogWarning("[EnemyManager] El jugador no tiene DetectorObjetivoJugador.");
    }

    // ─── Registro de enemigos ─────────────────────────────────────────────────

    public void RegistrarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null || todosLosEnemigos.Contains(enemigo)) return;
        todosLosEnemigos.Add(enemigo);
    }

    public void DesregistrarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null) return;
        todosLosEnemigos.Remove(enemigo);
        enemigosAtacando.Remove(enemigo);
        ultimoLogDeFalla.Remove(enemigo);
    }

    // ─── Loops ────────────────────────────────────────────────────────────────
    // Cada loop solo espera y delega en un método "Tick". Una coroutine que lanza una
    // excepción sin capturar MUERE y no vuelve a arrancar: por eso la lógica vive fuera de ella.

    private IEnumerator AILoop()
    {
        var espera = new WaitForSeconds(intervaloEvaluacionAI);
        while (true)
        {
            yield return espera;
            TickEvaluacionAI();
        }
    }

    private IEnumerator BloqueoEsquiveLoop()
    {
        var espera = new WaitForSeconds(intervaloBloqueoEsquive);
        while (true)
        {
            yield return espera;
            ParaCadaEnemigo(e => e.VerificarBloqueoYEsquive());
        }
    }

    private IEnumerator GestionarSlotsLoop()
    {
        var espera = new WaitForSeconds(intervaloGestionSlots);
        while (true)
        {
            yield return espera;
            TickSlots();
        }
    }

    private void TickEvaluacionAI()
    {
        if (Jugador == null)
        {
            ActualizarJugador();
            if (Jugador == null) return;
        }

        LimpiarEnemigos();
        ParaCadaEnemigo(e => e.EvaluarComportamiento());
    }

    /// <summary>
    /// Ejecuta una acción sobre cada enemigo vivo, aislando los errores.
    /// Se recorre una COPIA de la lista: durante la acción un enemigo puede morir y
    /// desregistrarse, y modificar una lista mientras se recorre con foreach lanza excepción.
    /// </summary>
    private void ParaCadaEnemigo(Action<Enemigo> accion)
    {
        snapshot.Clear();
        snapshot.AddRange(todosLosEnemigos);

        foreach (var e in snapshot)
        {
            if (e == null || e.EstaMuerto()) continue;

            try
            {
                accion(e);
            }
            catch (Exception ex)
            {
                ReportarFallaDeIA(e, ex);
            }
        }
    }

    /// <summary>
    /// El enemigo con fallo SIGUE registrado (desregistrarlo lo dejaba sin IA y persiguiendo al jugador
    /// para siempre) y se le pide recuperarse. El log se limita a uno cada pocos segundos por enemigo
    /// para no inundar la consola, pero conserva el stack trace y selecciona al enemigo al hacer clic.
    /// </summary>
    private void ReportarFallaDeIA(Enemigo e, Exception ex)
    {
        float ahora = Time.time;
        if (!ultimoLogDeFalla.TryGetValue(e, out float ultimo) || ahora - ultimo >= IntervaloLogFalla)
        {
            ultimoLogDeFalla[e] = ahora;
            Debug.LogException(ex, e);
        }

        e.RecuperarDeError();
    }

    // ─── Slots de ataque ──────────────────────────────────────────────────────

    private void TickSlots()
    {
        LimpiarEnemigos();
        ActualizarListaAtacantes();
        LiberarAtacantesAtascados();
        ClasificarCandidatos();

        AsignarSlots(candidatosMelee,     maxEnemigosMeleeAtacandoSimultaneamente - ContarAtacantes(arqueros: false));
        AsignarSlots(candidatosDistancia, maxArquerosAtacandoSimultaneamente      - ContarAtacantes(arqueros: true));

        // Los candidatos melee que sobran son los que no recibieron slot: ¿alguno esta mucho mas cerca que un atacante?
        IntercambiarPorMasCercano(candidatosMelee);
    }

    /// <summary>
    /// Quita el slot a quien lo tiene hace demasiado sin iniciar un combo (no esta atacando de verdad).
    /// Se recorre hacia atras porque LiberarEnemigo borra de la lista que se esta recorriendo.
    /// </summary>
    private void LiberarAtacantesAtascados()
    {
        for (int i = enemigosAtacando.Count - 1; i >= 0; i--)
        {
            Enemigo a = enemigosAtacando[i];
            if (a == null || !a.ParticipaEnIntercambioDeSlots || a.EstaEnCombo) continue;
            if (a.TiempoConSlot < tiempoMaximoConSlot) continue;

            a.EnfriarAtaque(enfriamientoTrasLiberar);
            LiberarEnemigo(a);
        }
    }

    /// <summary>
    /// Los slots melee van a quienes estan mas cerca del jugador. Cada tick se hace como maximo
    /// un cambio: el atacante mas lejano (que no este en pleno combo) cede su slot al candidato
    /// mas cercano, y solo si la diferencia supera margenCambioSlot (histeresis).
    /// </summary>
    private void IntercambiarPorMasCercano(List<Enemigo> candidatos)
    {
        if (Jugador == null || candidatos.Count == 0) return;

        Enemigo peorAtacante = null;
        float   peorDistancia = float.MinValue;

        foreach (var a in enemigosAtacando)
        {
            if (a == null || a is EnemigoDistancia) continue;
            if (!a.ParticipaEnIntercambioDeSlots || a.EstaEnCombo) continue;

            float d = DistanciaAlJugador(a);
            if (d > peorDistancia) { peorDistancia = d; peorAtacante = a; }
        }
        if (peorAtacante == null) return;

        Enemigo mejorCandidato  = null;
        float   mejorDistancia  = float.MaxValue;

        foreach (var c in candidatos)
        {
            float d = DistanciaAlJugador(c);
            if (d < mejorDistancia) { mejorDistancia = d; mejorCandidato = c; }
        }
        if (mejorCandidato == null || mejorDistancia + margenCambioSlot >= peorDistancia) return;

        peorAtacante.EnfriarAtaque(enfriamientoTrasLiberar);
        LiberarEnemigo(peorAtacante);
        mejorCandidato.OrdenarAtacar();
    }

    private float DistanciaAlJugador(Enemigo e) =>
        Vector3.Distance(e.transform.position, Jugador.transform.position);

    /// <summary>Separa en melee y distancia a los enemigos que pueden recibir un slot ahora mismo.</summary>
    private void ClasificarCandidatos()
    {
        candidatosMelee.Clear();
        candidatosDistancia.Clear();

        foreach (var e in todosLosEnemigos)
        {
            if (!EsCandidato(e)) continue;

            if (e is EnemigoDistancia) candidatosDistancia.Add(e);
            else                       candidatosMelee.Add(e);
        }
    }

    private bool EsCandidato(Enemigo e)
    {
        if (e == null)                                      return false;
        if (enemigosAtacando.Contains(e))                   return false;
        if (!e.EstaDisponibleParaAtacar())                  return false;
        if (!e.detectarJugador.SePuedeDetectarAlJugador())  return false;

        var salud = e.GetHealthComp();
        return salud == null
            || !(salud.EstaSiendoDanado || salud.EstaStuneado || salud.EnGuardBreak || salud.getBloqueando());
    }

    private void AsignarSlots(List<Enemigo> candidatos, int slotsLibres)
    {
        for (int i = 0; i < slotsLibres && candidatos.Count > 0; i++)
        {
            Enemigo mejor = SeleccionarMejor(candidatos);
            if (mejor == null) return;

            mejor.OrdenarAtacar();
            candidatos.Remove(mejor);
        }
    }

    private Enemigo SeleccionarMejor(List<Enemigo> candidatos)
    {
        Enemigo mejor  = null;
        float   mejorU = float.MinValue;

        foreach (var c in candidatos)
        {
            if (c == null || c.utilityGrupal == null) continue;

            float u = c.utilityGrupal.CalcularUtilidadAtacar();
            if (u > mejorU) { mejorU = u; mejor = c; }
        }
        return mejor;
    }

    private int ContarAtacantes(bool arqueros)
    {
        int cantidad = 0;
        foreach (var e in enemigosAtacando)
            if ((e is EnemigoDistancia) == arqueros) cantidad++;
        return cantidad;
    }

    // ─── Limpieza de listas ───────────────────────────────────────────────────

    private void LimpiarEnemigos()
    {
        todosLosEnemigos.RemoveAll(e => e == null || e.EstaMuerto());
        enemigosAtacando.RemoveAll(e => e == null || e.EstaMuerto() || !e.EstaAtacando());
    }

    private void ActualizarListaAtacantes()
    {
        enemigosAtacando.Clear();
        foreach (var e in todosLosEnemigos)
            if (e != null && e.EstaAtacando()) enemigosAtacando.Add(e);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void ActualizarJugador() =>
        AsignarJugador(GameObject.FindGameObjectWithTag("Player"));

    public void LiberarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null) return;
        enemigo.TerminarAtaque();
        enemigosAtacando.Remove(enemigo);
    }

    public void ResetearEstadoDeOleada() => enemigosAtacando.Clear();

    public bool AreAllEnemiesDead()
    {
        LimpiarEnemigos();
        return todosLosEnemigos.Count == 0;
    }

    public int ContarEnemigosAtacando()
    {
        ActualizarListaAtacantes();
        return enemigosAtacando.Count;
    }

    public bool HaySlotsDisponibles()
    {
        ActualizarListaAtacantes();
        return enemigosAtacando.Count < maxEnemigosMeleeAtacandoSimultaneamente;
    }

    // ─── Rodeo ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Punto sobre un círculo alrededor del jugador para un enemigo que rodea (no ataca).
    /// El ángulo parte del lado en el que el enemigo YA está respecto al jugador, así que su
    /// trayecto nunca cruza al jugador. Solo se corrige para separarlo de otros que rodean y
    /// se le da un pequeño avance orbital.
    /// </summary>
    public Vector3 ObtenerPosicionParaRodear(Enemigo enemigo, float radioDeseado = 8f)
    {
        if (Jugador == null) return enemigo.transform.position;

        Vector3 centro       = Jugador.transform.position;
        float   anguloPropio = AnguloAlrededorDelJugador(enemigo.transform.position, centro);

        float angulo = anguloPropio
                     + CalcularEmpujeSeparacion(enemigo, anguloPropio, centro)
                     + DireccionOrbital(enemigo) * derivaOrbitalPorActualizacion;

        float   rad       = angulo * Mathf.Deg2Rad;
        Vector3 direccion = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
        Vector3 posicion  = centro + direccion * radioDeseado;

        return NavMesh.SamplePosition(posicion, out NavMeshHit hit, radioDeseado * 0.5f, NavMesh.AllAreas)
            ? hit.position
            : centro + direccion * (radioDeseado * 0.7f);
    }

    /// <summary>Ángulo (grados, en el plano XZ) en el que está una posición respecto al jugador.</summary>
    private static float AnguloAlrededorDelJugador(Vector3 posicion, Vector3 centro)
    {
        Vector3 d = posicion - centro;
        return Mathf.Atan2(d.z, d.x) * Mathf.Rad2Deg;
    }

    /// <summary>Empuja al enemigo lejos (en ángulo) de los vecinos que rodean demasiado cerca de él.</summary>
    private float CalcularEmpujeSeparacion(Enemigo enemigo, float anguloPropio, Vector3 centro)
    {
        float empuje = 0f;

        foreach (var e in todosLosEnemigos)
        {
            if (e == null || e == enemigo || e.EstaMuerto() || e.EstaAtacando()) continue;

            // DeltaAngle = diferencia más corta con signo (-180..180) entre dos ángulos.
            float delta    = Mathf.DeltaAngle(anguloPropio, AnguloAlrededorDelJugador(e.transform.position, centro));
            float absDelta = Mathf.Abs(delta);
            if (absDelta >= separacionAngularMinima) continue;

            float sentido = absDelta < 0.01f ? DireccionOrbital(enemigo) : -Mathf.Sign(delta);
            empuje += sentido * (separacionAngularMinima - absDelta) * 0.5f;
        }

        return Mathf.Clamp(empuje, -separacionAngularMinima, separacionAngularMinima);
    }

    /// <summary>Cada enemigo orbita siempre en el mismo sentido (+1 / -1), estable entre llamadas.</summary>
    private static float DireccionOrbital(Enemigo enemigo) =>
        (enemigo.GetInstanceID() & 1) == 0 ? 1f : -1f;
}