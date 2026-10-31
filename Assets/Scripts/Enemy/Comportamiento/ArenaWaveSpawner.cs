using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TipoEnemigoArena
{
    [Tooltip("Prefab del enemigo a instanciar.")]
    public GameObject prefab;

    [Tooltip("Nombre legible para mensajes de error.")]
    public string nombre = "Enemigo";

    [Tooltip("Oleada a partir de la cual este tipo puede aparecer (1 = desde el inicio).")]
    public int oleadaDeAparicion = 1;

    [Tooltip("Porcentaje del total de enemigos que será de este tipo (0-1). " +
             "El primer tipo registrado absorbe los enemigos sobrantes del redondeo.")]
    [Range(0f, 1f)]
    public float porcentajeDelTotal = 1f;
}

/// <summary>
/// Flujo de oleadas infinitas del modo arena: decide cuántos y qué enemigos aparecen,
/// los instancia con VFX de portal, muestra tutoriales y avanza de oleada cuando todos mueren.
/// La IA y el combate de los enemigos NO son responsabilidad de este script.
/// </summary>
public class ArenaWaveSpawner : MonoBehaviour
{
    [Header("Tipos de Enemigo")]
    [Tooltip("Tipos disponibles. El primero es el 'principal' y absorbe sobrantes de redondeo.")]
    [SerializeField] private List<TipoEnemigoArena> tiposDeEnemigo = new();

    [Header("Escalado de Oleadas")]
    [Tooltip("Número de enemigos en la oleada 1.")]
    [SerializeField] private int enemigosIniciales = 3;

    [Tooltip("Enemigos adicionales por cada oleada completada.")]
    [SerializeField] private int incrementoPorOleada = 2;

    [Tooltip("Máximo de enemigos en una sola oleada.")]
    [SerializeField] private int maximoEnemigosPorOleada = 20;

    [Header("Puntos de Aparición")]
    [Tooltip("Transforms vacíos donde aparecen los enemigos. Cada uno debe tener como primer hijo el VFX de portal.")]
    [SerializeField] private List<Transform> spawnPoints = new();

    [Tooltip("Tiempo que el VFX de portal permanece activo antes de aparecer el enemigo.")]
    [SerializeField] private float tiempoVFXAntesDeSpawn = 0.8f;

    [Tooltip("Tiempo entre la aparición de cada enemigo individual.")]
    [SerializeField] private float tiempoEntreSpawns = 0.3f;

    [Header("Tutoriales por Oleada")]
    [Tooltip("Índices de paneles de tutorial. Los paneles 0 y 1 se muestran en la oleada 1; " +
             "desde la oleada 2 se muestra uno por oleada (oleada N → índice N). Si no hay más, no se muestra nada.")]
    [SerializeField] private List<int> indicesPanelesTutorial = new();

    [Tooltip("Tiempo entre el primer y segundo panel tutorial de la oleada 1.")]
    [SerializeField] private float tiempoEntreDosPrimerosTutoriales = 5f;

    [Header("Ritmo")]
    [Tooltip("Pausa entre que muere el último enemigo y empieza la siguiente oleada.")]
    [SerializeField] private float tiempoEntreOleadas = 3f;

    [Tooltip("Cada cuántos segundos se verifica si todos los enemigos murieron.")]
    [SerializeField] private float intervaloVerificacionMuertes = 0.5f;

    // Otros sistemas (HUD, cámara, audio) se suscriben sin acoplarse al spawner. Parámetro: número de oleada.
    public static event System.Action<int> OnOleadaIniciada;
    public static event System.Action<int> OnOleadaCompletada;

    private int oleadaActual;
    private bool oleadaEnCurso;

    // Un VFX por spawn point (primer hijo); null si el punto no lo tiene.
    private readonly List<GameObject> vfxPortales = new();

    public int OleadaActual   => oleadaActual;
    public bool OleadaEnCurso => oleadaEnCurso;

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void Start()
    {
        ValidarConfiguracion();
        CachearVFXPortales();
        StartCoroutine(FlujoDeOleadas());
    }

    private void ValidarConfiguracion()
    {
        if (tiposDeEnemigo.Count == 0)
            Debug.LogError("[ArenaWaveSpawner] No hay tipos de enemigo configurados.");

        if (spawnPoints.Count == 0)
            Debug.LogError("[ArenaWaveSpawner] No hay spawn points asignados.");

        float totalPorcentaje = 0f;
        foreach (var tipo in tiposDeEnemigo)
        {
            if (tipo.prefab == null)
                Debug.LogError($"[ArenaWaveSpawner] El tipo '{tipo.nombre}' no tiene prefab asignado.");
            totalPorcentaje += tipo.porcentajeDelTotal;
        }

        if (totalPorcentaje > 1.01f)
            Debug.LogWarning($"[ArenaWaveSpawner] Los porcentajes suman {totalPorcentaje:F2}; deberían sumar 1.0 o menos.");
    }

    private void CachearVFXPortales()
    {
        foreach (var punto in spawnPoints)
        {
            GameObject vfx = punto.childCount > 0 ? punto.GetChild(0).gameObject : null;
            if (vfx == null)
                Debug.LogWarning($"[ArenaWaveSpawner] '{punto.name}' no tiene hijo VFX.");
            vfxPortales.Add(vfx);
        }
    }

    // ─── Flujo de oleadas ─────────────────────────────────────────────────────

    private IEnumerator FlujoDeOleadas()
    {
        // Un frame para que EnemyManager termine su inicialización.
        yield return null;

        while (true)
        {
            oleadaActual++;
            oleadaEnCurso = true;
            OnOleadaIniciada?.Invoke(oleadaActual);

            yield return StartCoroutine(MostrarTutorialDeOleada(oleadaActual));
            yield return StartCoroutine(SpawnearOleada(CalcularTotalEnemigos(oleadaActual)));
            yield return StartCoroutine(EsperarFinDeOleada());

            oleadaEnCurso = false;
            OnOleadaCompletada?.Invoke(oleadaActual);

            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }

    /// <summary>Escalado lineal (base + incremento por oleada, con techo): predecible y fácil de reportar.</summary>
    private int CalcularTotalEnemigos(int numeroOleada)
    {
        int total = enemigosIniciales + (numeroOleada - 1) * incrementoPorOleada;
        return Mathf.Min(total, maximoEnemigosPorOleada);
    }

    /// <summary>
    /// Reparte el total entre los tipos disponibles (oleadaDeAparicion &lt;= oleada actual) según su porcentaje.
    /// El tipo principal (índice 0) absorbe el redondeo para que el total sea exacto.
    /// </summary>
    private List<(TipoEnemigoArena tipo, int cantidad)> DistribuirEnemigos(int total, int numeroOleada)
    {
        var disponibles = new List<TipoEnemigoArena>();
        foreach (var tipo in tiposDeEnemigo)
            if (tipo.oleadaDeAparicion <= numeroOleada && tipo.prefab != null)
                disponibles.Add(tipo);

        var distribucion = new List<(TipoEnemigoArena, int)>();
        if (disponibles.Count == 0) return distribucion;

        if (disponibles.Count == 1)
        {
            distribucion.Add((disponibles[0], total));
            return distribucion;
        }

        int asignados = 0;
        for (int i = 1; i < disponibles.Count; i++)
        {
            int cantidad = Mathf.FloorToInt(total * disponibles[i].porcentajeDelTotal);
            distribucion.Add((disponibles[i], cantidad));
            asignados += cantidad;
        }

        distribucion.Insert(0, (disponibles[0], total - asignados));
        return distribucion;
    }

    // ─── Spawn ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Instancia los enemigos intercalando tipos y repartiéndolos de forma circular entre los spawn points.
    /// El portal de cada punto se activa antes de su primer enemigo y se apaga al final.
    /// </summary>
    private IEnumerator SpawnearOleada(int totalEnemigos)
    {
        if (spawnPoints.Count == 0 || totalEnemigos == 0) yield break;

        if (EnemyManager.instance != null) EnemyManager.instance.ResetearEstadoDeOleada();

        var distribucion = DistribuirEnemigos(totalEnemigos, oleadaActual);
        if (distribucion.Count == 0)
        {
            Debug.LogError("[ArenaWaveSpawner] No hay tipos de enemigo disponibles para esta oleada.");
            yield break;
        }

        var colaDeSpawn = ConstruirColaIntercalada(distribucion);
        var spawnPointsEnUso = new HashSet<int>();

        for (int i = 0; i < colaDeSpawn.Count; i++)
        {
            int spIdx = i % spawnPoints.Count;

            if (spawnPointsEnUso.Add(spIdx))
                AlternarVFXPortal(spIdx, true);

            // Espera el VFX en el primer enemigo y al reiniciar la vuelta de spawn points; si no, el intervalo corto.
            bool esperaVFX = i == 0 || !spawnPointsEnUso.Contains(spIdx - 1);
            yield return new WaitForSeconds(esperaVFX ? tiempoVFXAntesDeSpawn : tiempoEntreSpawns);

            InstanciarEnemigo(colaDeSpawn[i], spawnPoints[spIdx]);
        }

        yield return new WaitForSeconds(tiempoEntreSpawns);
        foreach (int spIdx in spawnPointsEnUso)
            AlternarVFXPortal(spIdx, false);
    }

    /// <summary>Intercala los tipos (A, B, A, B...) para que la oleada no salga "primero todos los melee".</summary>
    private static List<GameObject> ConstruirColaIntercalada(List<(TipoEnemigoArena tipo, int cantidad)> distribucion)
    {
        int maxCantidad = 0;
        foreach (var (_, cantidad) in distribucion)
            maxCantidad = Mathf.Max(maxCantidad, cantidad);

        var cola = new List<GameObject>();
        for (int i = 0; i < maxCantidad; i++)
            foreach (var (tipo, cantidad) in distribucion)
                if (i < cantidad)
                    cola.Add(tipo.prefab);

        return cola;
    }

    /// <summary>
    /// Se registra aquí además de en Enemigo.Start(): garantiza que el enemigo esté en la lista
    /// del EnemyManager antes del siguiente tick de la IA.
    /// </summary>
    private void InstanciarEnemigo(GameObject prefab, Transform punto)
    {
        if (prefab == null || punto == null) return;

        GameObject instancia = Instantiate(prefab, punto.position, punto.rotation);
        Enemigo enemigo = instancia.GetComponent<Enemigo>();

        if (enemigo == null)
        {
            Debug.LogError($"[ArenaWaveSpawner] El prefab '{prefab.name}' no tiene componente Enemigo.");
            Destroy(instancia);
            return;
        }

        if (EnemyManager.instance != null)
            EnemyManager.instance.RegistrarEnemigo(enemigo);
        else
            Debug.LogError("[ArenaWaveSpawner] EnemyManager no encontrado al registrar enemigo.");
    }

    private void AlternarVFXPortal(int indice, bool activo)
    {
        if (indice < 0 || indice >= vfxPortales.Count) return;

        GameObject vfx = vfxPortales[indice];
        if (vfx != null) vfx.SetActive(activo);
    }

    // ─── Fin de oleada ────────────────────────────────────────────────────────

    /// <summary>
    /// Polling en vez de evento: Enemigo no notifica su muerte y el intervalo de 0.5 s es imperceptible.
    /// La primera espera da tiempo a que los recién spawneados se registren antes de contar cero.
    /// </summary>
    private IEnumerator EsperarFinDeOleada()
    {
        yield return new WaitForSeconds(intervaloVerificacionMuertes);

        while (EnemyManager.instance != null && !EnemyManager.instance.AreAllEnemiesDead())
            yield return new WaitForSeconds(intervaloVerificacionMuertes);
    }

    // ─── Tutoriales ───────────────────────────────────────────────────────────

    /// <summary>Se muestra antes de spawnear para que el jugador tenga tiempo de leer.</summary>
    private IEnumerator MostrarTutorialDeOleada(int numeroOleada)
    {
        if (indicesPanelesTutorial.Count == 0 || MenuManager.Instance == null) yield break;

        if (numeroOleada == 1)
        {
            MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[0]);

            if (indicesPanelesTutorial.Count > 1)
            {
                yield return new WaitForSeconds(tiempoEntreDosPrimerosTutoriales);
                MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[1]);
            }
            yield break;
        }

        // Oleada N (N >= 2) usa el panel de índice N; los paneles 0 y 1 ya se usaron en la oleada 1.
        if (numeroOleada < indicesPanelesTutorial.Count)
            MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[numeroOleada]);
    }
}