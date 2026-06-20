using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TipoEnemigoArena
{
    [Tooltip("Prefab del enemigo a instanciar.")]
    public GameObject prefab;

    [Tooltip("Nombre legible para logs y debugging.")]
    public string nombre = "Enemigo";

    [Tooltip("Oleada a partir de la cual este tipo puede aparecer (1 = desde el inicio).")]
    public int oleadaDeAparicion = 1;

    [Tooltip("Porcentaje del total de enemigos que será de este tipo (0-1). " +
             "El primer tipo registrado absorbe los enemigos sobrantes del redondeo.")]
    [Range(0f, 1f)]
    public float porcentajeDelTotal = 1f;
}

// ─── Spawner principal ────────────────────────────────────────────────────────

/// <summary>
/// Gestiona el flujo de oleadas infinitas del modo arena.
/// 
/// RESPONSABILIDADES:
///   - Decidir cuántos y qué tipo de enemigos spawnear por oleada.
///   - Instanciar enemigos y registrarlos en EnemyManager.
///   - Controlar el VFX de spawn en cada punto de aparición.
///   - Activar paneles de tutorial asociados a cada oleada.
///   - Detectar cuándo todos los enemigos murieron para avanzar de oleada.
/// 
/// NO ES RESPONSABILIDAD DE ESTE SCRIPT:
///   - La IA de los enemigos (EnemyManager).
///   - La lógica de combate (Enemigo / HealthComp).
///   - Mostrar UI de progreso de oleada (delegar a otro componente vía evento).
/// </summary>
public class ArenaWaveSpawner : MonoBehaviour
{
    // ─── Configuración de enemigos ────────────────────────────────────────────

    [Header("Tipos de Enemigo")]
    [Tooltip("Lista de tipos de enemigo disponibles. El orden importa: el primero " +
             "es el 'principal' y absorbe sobrantes de redondeo.")]
    [SerializeField] private List<TipoEnemigoArena> tiposDeEnemigo = new();

    // ─── Configuración de escalado ────────────────────────────────────────────

    [Header("Escalado de Oleadas")]
    [Tooltip("Número de enemigos en la oleada 1.")]
    [SerializeField] private int enemigosIniciales = 3;

    [Tooltip("Enemigos adicionales que se agregan por cada oleada completada.")]
    [SerializeField] private int incrementoPorOleada = 2;

    [Tooltip("Número máximo de enemigos que puede haber en una sola oleada. " +
             "Evita que el juego se vuelva injugable en oleadas muy avanzadas.")]
    [SerializeField] private int maximoEnemigosPorOleada = 20;

    // ─── Spawn points ─────────────────────────────────────────────────────────

    [Header("Puntos de Aparición")]
    [Tooltip("Transforms vacíos que marcan dónde aparecen los enemigos. " +
             "Cada uno debe tener un ParticleSystem como hijo para el VFX de portal.")]
    [SerializeField] private List<Transform> spawnPoints = new();

    [Tooltip("Tiempo que el VFX de portal permanece activo antes de aparecer el enemigo.")]
    [SerializeField] private float tiempoVFXAntesDeSpawn = 0.8f;

    [Tooltip("Tiempo entre la aparición de cada enemigo individual (para que no aparezcan todos a la vez).")]
    [SerializeField] private float tiempoEntreSpawns = 0.3f;

    // ─── Tutorial ─────────────────────────────────────────────────────────────

    [Header("Tutoriales por Oleada")]
    [Tooltip("Índices de paneles de tutorial a mostrar. " +
             "El panel 0 y 1 se muestran en la oleada 1 (excepción especial). " +
             "A partir del panel 2, uno por oleada. " +
             "Si hay más oleadas que paneles, no se muestra nada.")]
    [SerializeField] private List<int> indicesPanelesTutorial = new();

    [Tooltip("Tiempo entre el primer y segundo panel tutorial de la oleada 1.")]
    [SerializeField] private float tiempoEntreDosPrimerosTutoriales = 5f;

    // ─── Pausa entre oleadas ──────────────────────────────────────────────────

    [Header("Ritmo")]
    [Tooltip("Segundos de pausa entre que muere el último enemigo y empieza la siguiente oleada. " +
             "Da tiempo al jugador a respirar.")]
    [SerializeField] private float tiempoEntreOleadas = 3f;

    [Tooltip("Cada cuántos segundos se verifica si todos los enemigos murieron.")]
    [SerializeField] private float intervaloVerificacionMuertes = 0.5f;

    // ─── Estado interno ───────────────────────────────────────────────────────

    private int oleadaActual = 0;
    private bool oleadaEnCurso = false;

    // Cache de ParticleSystems: uno por spawn point (hijo directo).
    private List<ParticleSystem> vfxPortales = new();

    // ─── Eventos públicos ─────────────────────────────────────────────────────

    // Otros sistemas (HUD, cámara, audio) pueden suscribirse sin acoplarse al Spawner.
    public static event System.Action<int> OnOleadaIniciada;   // parámetro: número de oleada
    public static event System.Action<int> OnOleadaCompletada; // parámetro: número de oleada

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void Start()
    {
        ValidarConfiguracion();
        CachearVFXPortales();
        StartCoroutine(FlujoDeOleadas());
    }

    /// <summary>
    /// Verifica en editor que la configuración mínima esté completa.
    /// Falla rápido con mensajes claros en lugar de errores crípticos en runtime.
    /// </summary>
    private void ValidarConfiguracion()
    {
        if (tiposDeEnemigo.Count == 0)
            Debug.LogError("[ArenaWaveSpawner] No hay tipos de enemigo configurados.");

        if (spawnPoints.Count == 0)
            Debug.LogError("[ArenaWaveSpawner] No hay spawn points asignados.");

        foreach (var tipo in tiposDeEnemigo)
            if (tipo.prefab == null)
                Debug.LogError($"[ArenaWaveSpawner] El tipo '{tipo.nombre}' no tiene prefab asignado.");

        // Validar que los porcentajes no superen 1.0 en total
        float totalPorcentaje = 0f;
        foreach (var tipo in tiposDeEnemigo) totalPorcentaje += tipo.porcentajeDelTotal;
        if (totalPorcentaje > 1.01f)
            Debug.LogWarning($"[ArenaWaveSpawner] Los porcentajes de tipos suman {totalPorcentaje:F2}. " +
                             "Deberían sumar 1.0 o menos.");
    }

    /// <summary>
    /// Busca el ParticleSystem hijo de cada spawn point y lo guarda en cache.
    /// Si un spawn point no tiene ParticleSystem, deja null en esa posición
    /// (el spawn funciona igual, solo sin VFX).
    /// </summary>
    private void CachearVFXPortales()
    {
        foreach (var punto in spawnPoints)
        {
            var ps = punto != null ? punto.GetComponentInChildren<ParticleSystem>() : null;
            if (ps == null && punto != null)
                Debug.LogWarning($"[ArenaWaveSpawner] El spawn point '{punto.name}' no tiene ParticleSystem hijo. " +
                                 "El enemigo aparecerá sin VFX de portal.");
            vfxPortales.Add(ps);
        }
    }

    // ─── Flujo principal ──────────────────────────────────────────────────────

    /// <summary>
    /// Bucle principal del modo arena. Corre indefinidamente.
    /// 
    /// FLUJO POR OLEADA:
    ///   1. Incrementar contador y calcular enemigos.
    ///   2. Mostrar tutorial correspondiente.
    ///   3. Spawnear enemigos con VFX.
    ///   4. Esperar a que todos mueran (polling).
    ///   5. Pausa dramática.
    ///   6. Repetir.
    /// </summary>
    private IEnumerator FlujoDeOleadas()
    {
        // Esperar un frame para que EnemyManager termine su inicialización.
        yield return null;

        while (true)
        {
            oleadaActual++;
            oleadaEnCurso = true;

            int totalEnemigos = CalcularTotalEnemigos(oleadaActual);
            Debug.Log($"[ArenaWaveSpawner] Iniciando oleada {oleadaActual} con {totalEnemigos} enemigos.");

            // Notificar a sistemas externos (HUD, etc.)
            OnOleadaIniciada?.Invoke(oleadaActual);

            // Mostrar tutorial antes de spawnear
            yield return StartCoroutine(MostrarTutorialDeOleada(oleadaActual));

            // Spawnear todos los enemigos de esta oleada
            yield return StartCoroutine(SpawnearOleada(totalEnemigos));

            // Esperar a que todos los enemigos mueran
            yield return StartCoroutine(EsperarFinDeOleada());

            oleadaEnCurso = false;
            OnOleadaCompletada?.Invoke(oleadaActual);

            Debug.Log($"[ArenaWaveSpawner] Oleada {oleadaActual} completada. Pausa de {tiempoEntreOleadas}s.");
            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }

    // ─── Cálculo de enemigos ──────────────────────────────────────────────────

    /// <summary>
    /// Fórmula de escalado lineal: base + (oleada - 1) * incremento, con techo.
    /// 
    /// POR QUÉ LINEAL:
    /// Para un playtest de investigación, el escalado lineal es predecible y
    /// fácil de describir en el paper: "cada oleada añade N enemigos".
    /// Un escalado exponencial podría crear picos de dificultad difíciles de
    /// controlar antes del playtest.
    /// </summary>
    private int CalcularTotalEnemigos(int numeroOleada)
    {
        int total = enemigosIniciales + (numeroOleada - 1) * incrementoPorOleada;
        return Mathf.Min(total, maximoEnemigosPorOleada);
    }

    /// <summary>
    /// Distribuye el total de enemigos entre los tipos disponibles según sus porcentajes.
    /// 
    /// REGLA: Solo se incluyen tipos cuya oleadaDeAparicion <= oleadaActual.
    /// El tipo principal (índice 0) absorbe los sobrantes de redondeo para que
    /// el total siempre sea exacto.
    /// </summary>
    private List<(TipoEnemigoArena tipo, int cantidad)> DistribuirEnemigos(int total, int numeroOleada)
    {
        var disponibles = new List<TipoEnemigoArena>();
        foreach (var tipo in tiposDeEnemigo)
            if (tipo.oleadaDeAparicion <= numeroOleada && tipo.prefab != null)
                disponibles.Add(tipo);

        var distribucion = new List<(TipoEnemigoArena, int)>();
        if (disponibles.Count == 0) return distribucion;

        // Si solo hay un tipo disponible, todos los enemigos son de ese tipo.
        if (disponibles.Count == 1)
        {
            distribucion.Add((disponibles[0], total));
            return distribucion;
        }

        // Con múltiples tipos: calcular por porcentaje y redondear hacia abajo.
        int asignados = 0;
        for (int i = 1; i < disponibles.Count; i++)
        {
            int cantidad = Mathf.FloorToInt(total * disponibles[i].porcentajeDelTotal);
            distribucion.Add((disponibles[i], cantidad));
            asignados += cantidad;
        }

        // El tipo principal (índice 0) recibe el resto para que el total sea exacto.
        int cantidadPrincipal = total - asignados;
        distribucion.Insert(0, (disponibles[0], cantidadPrincipal));

        return distribucion;
    }

    // ─── Spawn ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Instancia todos los enemigos de la oleada, distribuyéndolos entre los
    /// spawn points disponibles de forma circular.
    /// 
    /// PATRÓN VFX:
    ///   Para cada spawn point que va a ser usado, activa el portal ANTES de
    ///   que aparezca el primer enemigo desde ese punto, y lo desactiva después
    ///   del último. Si varios enemigos usan el mismo punto, el portal permanece
    ///   activo durante toda la secuencia desde ese punto.
    /// </summary>
    private IEnumerator SpawnearOleada(int totalEnemigos)
    {
        if (spawnPoints.Count == 0 || totalEnemigos == 0) yield break;

        var distribucion = DistribuirEnemigos(totalEnemigos, oleadaActual);
        if (distribucion.Count == 0)
        {
            Debug.LogError("[ArenaWaveSpawner] No hay tipos de enemigo disponibles para esta oleada.");
            yield break;
        }

        // Construir lista plana de prefabs a spawnear, en orden intercalado por tipo.
        // Esto hace que la oleada se vea variada en vez de "primero todos los melee, luego los de distancia".
        var colaDeSpawn = new List<GameObject>();
        int maxCantidad = 0;
        foreach (var (_, cantidad) in distribucion)
            maxCantidad = Mathf.Max(maxCantidad, cantidad);

        for (int i = 0; i < maxCantidad; i++)
            foreach (var (tipo, cantidad) in distribucion)
                if (i < cantidad)
                    colaDeSpawn.Add(tipo.prefab);

        // Spawnear en secuencia, distribuyendo entre spawn points de forma circular.
        int indiceSpawnPoint = 0;
        var spawnPointsEnUso = new HashSet<int>();

        for (int i = 0; i < colaDeSpawn.Count; i++)
        {
            int spIdx = indiceSpawnPoint % spawnPoints.Count;
            Transform punto = spawnPoints[spIdx];

            // Activar VFX del portal si no estaba ya activo para este punto.
            if (!spawnPointsEnUso.Contains(spIdx))
            {
                spawnPointsEnUso.Add(spIdx);
                ActivarVFXPortal(spIdx);
            }

            // Esperar el tiempo del VFX antes del primer enemigo de este punto.
            // Para los siguientes del mismo punto, solo esperar entre spawns.
            if (i == 0 || !spawnPointsEnUso.Contains(spIdx - 1))
                yield return new WaitForSeconds(tiempoVFXAntesDeSpawn);
            else
                yield return new WaitForSeconds(tiempoEntreSpawns);

            InstanciarEnemigo(colaDeSpawn[i], punto);
            indiceSpawnPoint++;
        }

        // Desactivar todos los portales usados.
        yield return new WaitForSeconds(tiempoEntreSpawns);
        foreach (int spIdx in spawnPointsEnUso)
            DesactivarVFXPortal(spIdx);
    }

    /// <summary>
    /// Instancia un enemigo y lo registra en EnemyManager.
    /// 
    /// POR QUÉ REGISTRAR AQUÍ Y NO EN Enemigo.Start():
    /// Enemigo.Start() corre un frame después de la instanciación.
    /// RegistrarEnemigo() desde aquí garantiza que el enemigo esté en la lista
    /// del EnemyManager antes de que el AILoop lo evalúe.
    /// </summary>
    private void InstanciarEnemigo(GameObject prefab, Transform punto)
    {
        if (prefab == null || punto == null) return;

        GameObject instancia = Instantiate(prefab, punto.position, punto.rotation);
        Enemigo componente = instancia.GetComponent<Enemigo>();

        if (componente == null)
        {
            Debug.LogError($"[ArenaWaveSpawner] El prefab '{prefab.name}' no tiene componente Enemigo.");
            Destroy(instancia);
            return;
        }

        if (EnemyManager.instance != null)
            EnemyManager.instance.RegistrarEnemigo(componente);
        else
            Debug.LogError("[ArenaWaveSpawner] EnemyManager no encontrado al intentar registrar enemigo.");
    }

    // ─── VFX Portales ─────────────────────────────────────────────────────────

    private void ActivarVFXPortal(int indice)
    {
        if (indice < 0 || indice >= vfxPortales.Count) return;
        vfxPortales[indice]?.Play();
    }

    private void DesactivarVFXPortal(int indice)
    {
        if (indice < 0 || indice >= vfxPortales.Count) return;
        vfxPortales[indice]?.Stop();
    }

    // ─── Espera de fin de oleada ──────────────────────────────────────────────

    /// <summary>
    /// Hace polling cada intervaloVerificacionMuertes segundos hasta que
    /// EnemyManager confirma que no quedan enemigos vivos.
    /// 
    /// POR QUÉ POLLING Y NO EVENTO:
    /// Enemigo.cs no dispara eventos al morir (los detecta el EnemyManager
    /// internamente via LimpiarEnemigos). Modificar Enemigo.cs solo para esto
    /// introduciría acoplamiento en una clase estable. El polling a 0.5s es
    /// imperceptible para el jugador y mantiene Enemigo.cs sin cambios.
    /// </summary>
    private IEnumerator EsperarFinDeOleada()
    {
        // Esperar al menos un frame para que los enemigos recién spawneados
        // se registren en EnemyManager antes de verificar si hay cero.
        yield return new WaitForSeconds(intervaloVerificacionMuertes);

        while (EnemyManager.instance != null && !EnemyManager.instance.AreAllEnemiesDead())
            yield return new WaitForSeconds(intervaloVerificacionMuertes);
    }

    // ─── Sistema de tutoriales ────────────────────────────────────────────────

    /// <summary>
    /// Activa los paneles de tutorial correspondientes a la oleada actual.
    /// 
    /// LÓGICA ESPECIAL PARA OLEADA 1:
    ///   Oleada 1 → muestra paneles 0 y 1 (con pausa entre ellos).
    ///   Oleada 2 → muestra panel 2.
    ///   Oleada 3 → muestra panel 3.
    ///   ... y así sucesivamente.
    ///   Si ya no hay más paneles, no se muestra nada (sin errores).
    /// 
    /// POR QUÉ ACTIVAR ANTES DE SPAWNEAR:
    ///   El tutorial explica qué viene. Si se activa al mismo tiempo que los
    ///   enemigos aparecen, el jugador no tiene tiempo de leer.
    /// </summary>
    private IEnumerator MostrarTutorialDeOleada(int numeroOleada)
    {
        if (indicesPanelesTutorial.Count == 0) yield break;
        if (MenuManager.Instance == null) yield break;

        if (numeroOleada == 1)
        {
            // Excepción: primera oleada muestra los dos primeros paneles.
            if (indicesPanelesTutorial.Count > 0)
            {
                MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[0]);
                Debug.Log($"[ArenaWaveSpawner] Tutorial oleada 1 - panel {indicesPanelesTutorial[0]}");

                if (indicesPanelesTutorial.Count > 1)
                {
                    yield return new WaitForSeconds(tiempoEntreDosPrimerosTutoriales);
                    MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[1]);
                    Debug.Log($"[ArenaWaveSpawner] Tutorial oleada 1 - panel {indicesPanelesTutorial[1]}");
                }
            }
        }
        else
        {
            // Oleada N (N >= 2) usa el panel en índice (N + 0), es decir:
            // oleada 2 → índice 2, oleada 3 → índice 3, etc.
            int indicePanelParaEstaOleada = numeroOleada; // panel 0 y 1 ya usados en oleada 1
            if (indicePanelParaEstaOleada < indicesPanelesTutorial.Count)
            {
                MenuManager.Instance.AbrirPanelTutorial(indicesPanelesTutorial[indicePanelParaEstaOleada]);
                Debug.Log($"[ArenaWaveSpawner] Tutorial oleada {numeroOleada} - panel {indicesPanelesTutorial[indicePanelParaEstaOleada]}");
            }
            // Si no hay más paneles, simplemente no se muestra nada.
        }
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public int OleadaActual => oleadaActual;
    public bool OleadaEnCurso => oleadaEnCurso;
}
