using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Configuracion de Combate")]
    [SerializeField] private int   maxEnemigosAtacandoSimultaneamente = 2;
    [SerializeField] private float intervaloEvaluacionAI    = 0.1f;
    [SerializeField] private float intervaloGestionSlots    = 0.5f;
    [SerializeField] private float intervaloBloqueoEsquive  = 0.2f;

    public List<Enemigo> todosLosEnemigos = new List<Enemigo>();
    public List<Enemigo> enemigosAtacando = new List<Enemigo>();

    private Coroutine aiLoopCoroutine;
    private Coroutine gestionSlotsCoroutine;
    private Coroutine bloqueoEsquiveCoroutine;

    public GameObject Jugador;
    public DetectorObjetivoJugador DetectorObjetivo { get; private set; }

    // ─── Singleton ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void Start()
    {
        AsignarJugador(GameObject.FindGameObjectWithTag("Player"));
        StartCoroutine(InicializarDespuesDeUnFrame());
    }

    private IEnumerator InicializarDespuesDeUnFrame()
    {
        yield return null;

        todosLosEnemigos.Clear();
        enemigosAtacando.Clear();

        foreach (var enemigo in FindObjectsOfType<Enemigo>())
        {
            // FIX CRÍTICO: antes solo se agregaban al lista con AddRange.
            // BuscarJugador() nunca se llamaba, así que DetectarJugador.Player
            // quedaba null (el intento en Awake falla porque EnemyManager.Jugador
            // aún no está asignado en ese momento).
            // Con WaveManager esto no se notaba porque OnOleadaActivada sí llama
            // BuscarJugador(). Sin WaveManager, los enemigos nunca detectaban al jugador.
            enemigo.BuscarJugador();
            todosLosEnemigos.Add(enemigo);
        }

        ReiniciarCoroutines();
    }

    private void ReiniciarCoroutines()
    {
        if (aiLoopCoroutine         != null) StopCoroutine(aiLoopCoroutine);
        if (gestionSlotsCoroutine   != null) StopCoroutine(gestionSlotsCoroutine);
        if (bloqueoEsquiveCoroutine != null) StopCoroutine(bloqueoEsquiveCoroutine);

        aiLoopCoroutine         = StartCoroutine(AILoop());
        gestionSlotsCoroutine   = StartCoroutine(GestionarSlots());
        bloqueoEsquiveCoroutine = StartCoroutine(BloqueoEsquiveLoop());
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

        if (DetectorObjetivo == null)
            Debug.LogWarning("[EnemyManager] El jugador no tiene DetectorObjetivoJugador.");
    }

    // ─── API de oleadas ───────────────────────────────────────────────────────

    //DEPRECATED
    public void OnOleadaActivada(Transform oleada)
    {
        todosLosEnemigos.Clear();
        enemigosAtacando.Clear();

        foreach (var e in oleada.GetComponentsInChildren<Enemigo>(true))
        {
            e.BuscarJugador();
            todosLosEnemigos.Add(e);
        }
    }

    // ─── Registro dinámico ────────────────────────────────────────────────────

    public void RegistrarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null || todosLosEnemigos.Contains(enemigo)) return;
        enemigo.BuscarJugador();
        todosLosEnemigos.Add(enemigo);
    }

    public void DesregistrarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null) return;
        todosLosEnemigos.Remove(enemigo);
        enemigosAtacando.Remove(enemigo);
    }

    // ─── Loops de IA ─────────────────────────────────────────────────────────

    private IEnumerator AILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloEvaluacionAI);
            LimpiarEnemigos();
            foreach (var e in todosLosEnemigos)
            {
                if (e == null || e.EstaMuerto()) continue;
                e.EvaluarComportamiento();
            }
        }
    }

    private IEnumerator BloqueoEsquiveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloBloqueoEsquive);
            foreach (var e in todosLosEnemigos)
            {
                if (e == null || e.EstaMuerto()) continue;
                e.VerificarBloqueoYEsquive();
            }
        }
    }

    private IEnumerator GestionarSlots()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloGestionSlots);
            LimpiarEnemigos();
            ActualizarListaAtacantes();

            int slotsLibres = maxEnemigosAtacandoSimultaneamente - enemigosAtacando.Count;
            if (slotsLibres <= 0) continue;

            var candidatos = ObtenerCandidatos();
            if (candidatos.Count == 0) continue;

            if (enemigosAtacando.Count == 0)
            {
                Enemigo forzado = candidatos[Random.Range(0, candidatos.Count)];
                forzado.OrdenarAtacar();
                candidatos.Remove(forzado);
                slotsLibres--;
            }

            for (int i = 0; i < slotsLibres && candidatos.Count > 0; i++)
            {
                Enemigo mejor = SeleccionarMejor(candidatos);
                if (mejor == null) break;
                mejor.OrdenarAtacar();
                candidatos.Remove(mejor);
            }
        }
    }

    // ─── Limpieza y selección ─────────────────────────────────────────────────

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

    private List<Enemigo> ObtenerCandidatos()
    {
        var lista = new List<Enemigo>();
        foreach (var e in todosLosEnemigos)
        {
            if (e == null)                                      continue;
            if (enemigosAtacando.Contains(e))                  continue;
            if (!e.EstaDisponibleParaAtacar())                 continue;
            if (!e.detectarJugador.SePuedeDetectarAlJugador()) continue;
            var h = e.GetHealthComp();
            if (h != null && (h.EstaSiendoDanado || h.EstaStuneado || h.EnGuardBreak)) continue;
            lista.Add(e);
        }
        return lista;
    }

    private Enemigo SeleccionarMejor(List<Enemigo> candidatos)
    {
        Enemigo mejor   = null;
        float   mejorU  = float.MinValue;
        foreach (var c in candidatos)
        {
            if (c?.utilityGrupal == null) continue;
            float u = c.utilityGrupal.CalcularUtilidadAtacar();
            if (u > mejorU) { mejorU = u; mejor = c; }
        }
        return mejor;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void LiberarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null) return;
        enemigo.TerminarAtaque();
        enemigosAtacando.Remove(enemigo);
    }

    public void ActualizarJugador()
    {
        AsignarJugador(GameObject.FindGameObjectWithTag("Player"));
        foreach (var e in todosLosEnemigos) e?.BuscarJugador();
    }

    public bool AreAllEnemiesDead()      { LimpiarEnemigos(); return todosLosEnemigos.Count == 0; }
    public int  ContarEnemigosAtacando() { ActualizarListaAtacantes(); return enemigosAtacando.Count; }
    public bool HaySlotsDisponibles()    { ActualizarListaAtacantes(); return enemigosAtacando.Count < maxEnemigosAtacandoSimultaneamente; }

    public Vector3 ObtenerPosicionParaRodear(Enemigo enemigo, float radioDeseado = 8f)
    {
        if (Jugador == null) return enemigo.transform.position;

        var rodeando = new List<Enemigo>();
        foreach (var e in todosLosEnemigos)
            if (e != null && !e.EstaMuerto() && !enemigosAtacando.Contains(e))
                rodeando.Add(e);

        int   idx    = rodeando.IndexOf(enemigo); if (idx == -1) idx = 0;
        int   total  = Mathf.Max(rodeando.Count, 1);
        float angulo = (360f / total) * idx + Random.Range(-15f, 15f);
        float rad    = angulo * Mathf.Deg2Rad;
        float radio  = radioDeseado + Random.Range(-0.5f, 0.5f);

        Vector3 offset   = new Vector3(Mathf.Cos(rad) * radio, 0f, Mathf.Sin(rad) * radio);
        Vector3 posicion = Jugador.transform.position + offset;

        return NavMesh.SamplePosition(posicion, out NavMeshHit hit, radioDeseado * 0.5f, NavMesh.AllAreas)
            ? hit.position
            : Jugador.transform.position + offset.normalized * (radioDeseado * 0.7f);
    }
}