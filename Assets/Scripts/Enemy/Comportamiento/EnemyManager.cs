 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Configuracion de Combate")]
    [SerializeField] private int maxEnemigosAtacandoSimultaneamente = 2;
    [SerializeField] private float intervaloEvaluacionAI = 0.1f;
    [SerializeField] private float intervaloGestionSlots = 0.5f;

    public List<Enemigo> todosLosEnemigos = new List<Enemigo>();
    public List<Enemigo> enemigosAtacando = new List<Enemigo>();

    private Coroutine aiLoopCoroutine;
    private Coroutine gestionSlotsCoroutine;
    
    //otra referencias
    public GameObject Jugador;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Jugador = GameObject.FindGameObjectWithTag("Player");
        
        StartCoroutine(InicializarDespuesDeUnFrame());
    }

    private IEnumerator InicializarDespuesDeUnFrame()
    {
        yield return null;

        todosLosEnemigos.Clear();
        todosLosEnemigos.AddRange(FindObjectsOfType<Enemigo>());

        if (aiLoopCoroutine != null) StopCoroutine(aiLoopCoroutine);
        if (gestionSlotsCoroutine != null) StopCoroutine(gestionSlotsCoroutine);

        aiLoopCoroutine = StartCoroutine(AILoop());
        gestionSlotsCoroutine = StartCoroutine(GestionarSlots());
    }
    public void OnOleadaActivada(Transform oleada)
    {
        todosLosEnemigos.Clear();
        Enemigo[] enemigos = oleada.GetComponentsInChildren<Enemigo>(true);
        
        foreach(var e in enemigos) {
            e.BuscarJugador(); // Forzar búsqueda
            if(e.detectarJugador != null) e.detectarJugador.BuscarJugador();
        }
    
        todosLosEnemigos.AddRange(enemigos);
    }


    /// <summary>
    /// Loop de evaluacion de IA
    /// </summary>
    private IEnumerator AILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloEvaluacionAI);

            LimpiarEnemigos();

            foreach (var enemigo in todosLosEnemigos)
            {
                if (enemigo == null || enemigo.EstaMuerto()) continue;

                enemigo.EvaluarComportamiento();
            }
        }
    }

    /// <summary>
    /// Gestiona permisos de ataque
    /// </summary>
    private IEnumerator GestionarSlots()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloGestionSlots);

            LimpiarEnemigos();
            ActualizarListaAtacantes();

            int slotsLibres = maxEnemigosAtacandoSimultaneamente - enemigosAtacando.Count;

            if (slotsLibres > 0)
            {
                var candidatos = ObtenerCandidatos();

                if (candidatos.Count > 0 && enemigosAtacando.Count == 0)
                {
                    Debug.Log("[EnemyManager] Forzando ataque de al menos un enemigo");
                    Enemigo forzado = candidatos[Random.Range(0, candidatos.Count)];
                    forzado.OrdenarAtacar();
                    candidatos.Remove(forzado);
                    slotsLibres--;
                }

                for (int i = 0; i < slotsLibres && candidatos.Count > 0; i++)
                {
                    Enemigo mejor = SeleccionarMejor(candidatos);

                    if (mejor != null)
                    {
                        mejor.OrdenarAtacar();
                        candidatos.Remove(mejor);
                    }
                }
            }
        }
    }

    private void LimpiarEnemigos()
    {
        todosLosEnemigos.RemoveAll(e => e == null || e.EstaMuerto());
        enemigosAtacando.RemoveAll(e => e == null || e.EstaMuerto() || !e.EstaAtacando());
    }

    private void ActualizarListaAtacantes()
    {
        enemigosAtacando.Clear();

        foreach (var enemigo in todosLosEnemigos)
        {
            if (enemigo != null && enemigo.EstaAtacando())
            {
                enemigosAtacando.Add(enemigo);
            }
        }
    }

    private List<Enemigo> ObtenerCandidatos()
    {
        List<Enemigo> candidatos = new List<Enemigo>();

        foreach (var enemigo in todosLosEnemigos)
        {
            if (enemigo == null) continue;

            bool disponible = enemigo.EstaDisponibleParaAtacar() &&
                              !enemigosAtacando.Contains(enemigo);

            var health = enemigo.GetComponent<HealthComp>();
            if (health != null)
            {
                bool enEstadoCritico = health.EstaSiendoDanado ||
                                       health.EstaStuneado ||
                                       health.EnGuardBreak;

                if (enEstadoCritico)
                    disponible = false;
            }

            // Solo candidatos que detectan al jugador
            if (!enemigo.detectarJugador.SePuedeDetectarAlJugador())
                disponible = false;

            if (disponible)
            {
                candidatos.Add(enemigo);
            }
        }

        return candidatos;
    }

    private Enemigo SeleccionarMejor(List<Enemigo> candidatos)
    {
        if (candidatos.Count == 0) return null;

        Enemigo mejor = null;
        float mejorUtilidad = 0f;

        foreach (var candidato in candidatos)
        {
            if (candidato == null || candidato.utilityGrupal == null) continue;

            float utilidad = candidato.utilityGrupal.CalcularUtilidadAtacar();

            if (utilidad > mejorUtilidad)
            {
                mejorUtilidad = utilidad;
                mejor = candidato;
            }
        }

        return mejor;
    }

    public void LiberarEnemigo(Enemigo enemigo)
    {
        if (enemigo == null) return;

        enemigo.TerminarAtaque();
        enemigosAtacando.Remove(enemigo);
    }

    public void ActualizarJugador()
    {
        Jugador = GameObject.FindGameObjectWithTag("Player");
        
        foreach (var enemigo in todosLosEnemigos)
        {
            if (enemigo == null) continue;

            enemigo.BuscarJugador();

            DetectarJugador detector = enemigo.GetComponent<DetectarJugador>();
            if (detector != null)
            {
                detector.BuscarJugador();
            }
        }
    }

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
        return enemigosAtacando.Count < maxEnemigosAtacandoSimultaneamente;
    }

    /// <summary>
    /// Obtiene posicion para rodear distribuida alrededor del jugador
    /// </summary>
    public Vector3 ObtenerPosicionParaRodear(Enemigo enemigo, float radioDeseado = 8f)
    {
        if (Jugador == null) return enemigo.transform.position;

        Transform jugador = Jugador.transform;

        // Lista de enemigos rodeando
        List<Enemigo> enemigosRodeando = new List<Enemigo>();
        foreach (var e in todosLosEnemigos)
        {
            if (e != null && !e.EstaMuerto() && !enemigosAtacando.Contains(e))
            {
                enemigosRodeando.Add(e);
            }
        }

        int indice = enemigosRodeando.IndexOf(enemigo);
        if (indice == -1) indice = 0;

        int total = Mathf.Max(enemigosRodeando.Count, 1);

        float anguloBase = (360f / total) * indice;
        float variacion = Random.Range(-15f, 15f);
        anguloBase += variacion;

        float anguloRad = anguloBase * Mathf.Deg2Rad;
        float radioFinal = radioDeseado + Random.Range(-0.5f, 0.5f);

        Vector3 offset = new Vector3(
            Mathf.Cos(anguloRad) * radioFinal,
            0f,
            Mathf.Sin(anguloRad) * radioFinal
        );

        Vector3 posicionFinal = jugador.position + offset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(posicionFinal, out hit, radioDeseado * 0.5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return jugador.position + offset.normalized * (radioDeseado * 0.7f);
    }
}
