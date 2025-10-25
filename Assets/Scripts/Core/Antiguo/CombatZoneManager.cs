using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Maneja las zonas de combate, spawning de enemigos y oleadas del boss
/// </summary>
//public class CombatZoneManager : MonoBehaviour
//{
//    [Header("Configuración")]
//    [SerializeField] private Transform enemyContainer;
//    [SerializeField] private LayerMask enemyLayer;

//    [Header("Debug")]
//    [SerializeField] private bool showGizmos = true;
//    [SerializeField] private Color combatZoneColor = new Color(1f, 0f, 0f, 0.3f);

//    private CombatData currentCombatData;
//    private BossWavesData currentBossData;
//    private List<GameObject> activeEnemies = new List<GameObject>();
//    private bool isCombatActive = false;
//    private bool isBossFight = false;
//    private int currentWaveIndex = 0;
//    private Vector3 combatCenter;
//    private float combatRadius;

//    #region Setup

//    public void SetupCombatZone(CombatData combatData)
//    {
//        if (combatData == null)
//        {
//            Debug.LogWarning("[CombatZone] Combat data es nulo");
//            return;
//        }

//        CleanupCombat();

//        currentCombatData = combatData;
//        combatCenter = combatData.combatCenter;
//        combatRadius = combatData.combatRadius;
//        isCombatActive = true;
//        isBossFight = false;

//        StartCoroutine(SpawnEnemies(combatData.enemySpawns));
//    }

//    public void StartBossWaves(BossWavesData bossData)
//    {
//        if (bossData == null)
//        {
//            Debug.LogWarning("[CombatZone] Boss data es nulo");
//            return;
//        }

//        CleanupCombat();

//        currentBossData = bossData;
//        isCombatActive = true;
//        isBossFight = true;
//        currentWaveIndex = 0;

//        // Mostrar diálogo de intro del boss si existe
//        if (bossData.bossIntroDialogue != null)
//        {
//            StartCoroutine(PlayBossIntroAndStart());
//        }
//        else
//        {
//            StartNextWave();
//        }
//    }

//    #endregion

//    #region Enemy Spawning

//    private IEnumerator SpawnEnemies(List<EnemySpawnData> spawns)
//    {
//        foreach (EnemySpawnData spawnData in spawns)
//        {
//            if (spawnData.spawnDelay > 0)
//            {
//                yield return new WaitForSeconds(spawnData.spawnDelay);
//            }

//            SpawnEnemy(spawnData);
//        }

//        Debug.Log($"[CombatZone] {activeEnemies.Count} enemigos spawneados");
//    }

//    private void SpawnEnemy(EnemySpawnData spawnData)
//    {
//        if (spawnData.enemyPrefab == null)
//        {
//            Debug.LogWarning("[CombatZone] Enemy prefab es nulo");
//            return;
//        }

//        GameObject enemy = Instantiate(
//            spawnData.enemyPrefab,
//            spawnData.spawnPosition,
//            spawnData.spawnRotation,
//            enemyContainer != null ? enemyContainer : transform
//        );

//        // Suscribirse al evento de muerte del enemigo
//        Enemigo enemyHealth = enemy.GetComponent<Enemigo>();
//        if (enemyHealth != null)
//        {
//            //enemyHealth.OnEnemyDied += () => OnEnemyDied(enemy);
//        }
//        else
//        {
//            Debug.LogWarning($"[CombatZone] Enemigo {enemy.name} no tiene componente EnemyHealth");
//        }

//        activeEnemies.Add(enemy);
//        GameEvents.OnEnemySpawned?.Invoke(enemy);
//    }

//    #endregion

//    #region Boss Fight Waves

//    private IEnumerator PlayBossIntroAndStart()
//    {
//        // Aquí el DialogueManager debería manejar el diálogo
//        GameFlowController.Instance?.TriggerDialogue(currentBossData.bossIntroDialogue.dialogueId);

//        // Esperar un frame para que el diálogo se active
//        yield return null;

//        // Esperar a que termine el diálogo
//        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
//        if (dialogueManager != null)
//        {
//            while (dialogueManager.IsDialoguePlaying)
//            {
//                yield return null;
//            }
//        }

//        StartNextWave();
//    }

//    private void StartNextWave()
//    {
//        if (currentBossData == null || currentWaveIndex >= currentBossData.waves.Count)
//        {
//            // Todas las oleadas completadas, spawn del boss
//            SpawnBoss();
//            return;
//        }

//        WaveData wave = currentBossData.waves[currentWaveIndex];
//        Debug.Log($"[CombatZone] Iniciando oleada {currentWaveIndex + 1}: {wave.waveName}");

//        StartCoroutine(SpawnEnemies(wave.enemies));
//    }

//    private void OnWaveCompleted()
//    {
//        WaveData completedWave = currentBossData.waves[currentWaveIndex];

//        Debug.Log($"[CombatZone] Oleada {currentWaveIndex + 1} completada");
//        GameEvents.OnWaveCompleted?.Invoke(currentWaveIndex);

//        // Reproducir diálogo si existe
//        if (completedWave.waveCompletedDialogue != null)
//        {
//            StartCoroutine(PlayWaveDialogueAndContinue(completedWave));
//        }
//        else
//        {
//            ContinueToNextWave();
//        }
//    }

//    private IEnumerator PlayWaveDialogueAndContinue(WaveData wave)
//    {
//        GameFlowController.Instance?.TriggerDialogue(wave.waveCompletedDialogue.dialogueId);

//        yield return null;

//        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
//        if (dialogueManager != null)
//        {
//            while (dialogueManager.IsDialoguePlaying)
//            {
//                yield return null;
//            }
//        }

//        yield return new WaitForSeconds(wave.delayBeforeNextWave);

//        ContinueToNextWave();
//    }

//    private void ContinueToNextWave()
//    {
//        currentWaveIndex++;
//        StartNextWave();
//    }

//    private void SpawnBoss()
//    {
//        Debug.Log("[CombatZone] Spawneando Boss");

//        if (currentBossData.bossPrefab == null)
//        {
//            Debug.LogError("[CombatZone] Boss prefab es nulo!");
//            return;
//        }

//        EnemySpawnData bossSpawn = new EnemySpawnData
//        {
//            enemyPrefab = currentBossData.bossPrefab,
//            spawnPosition = currentBossData.bossSpawnPosition,
//            spawnRotation = Quaternion.identity,
//            spawnDelay = 0f
//        };

//        SpawnEnemy(bossSpawn);
//    }

//    #endregion

//    #region Enemy Death Handling

//    private void OnEnemyDied(GameObject enemy)
//    {
//        if (activeEnemies.Contains(enemy))
//        {
//            activeEnemies.Remove(enemy);
//            GameEvents.OnEnemyDefeated?.Invoke(enemy);

//            Debug.Log($"[CombatZone] Enemigo derrotado. Enemigos restantes: {activeEnemies.Count}");

//            // Verificar si todos los enemigos fueron derrotados
//            if (activeEnemies.Count == 0)
//            {
//                OnAllEnemiesDefeated();
//            }
//        }
//    }

//    private void OnAllEnemiesDefeated()
//    {
//        Debug.Log("[CombatZone] Todos los enemigos derrotados");

//        if (isBossFight)
//        {
//            // En boss fight, verificar si era el boss o una oleada
//            if (currentWaveIndex < currentBossData.waves.Count)
//            {
//                // Oleada completada
//                OnWaveCompleted();
//            }
//            else
//            {
//                // Boss derrotado
//                OnBossDefeated();
//            }
//        }
//        else
//        {
//            // Combate normal completado
//            isCombatActive = false;
//            GameEvents.OnAllEnemiesDefeated?.Invoke();
//        }
//    }

//    private void OnBossDefeated()
//    {
//        Debug.Log("[CombatZone] Boss derrotado!");

//        isCombatActive = false;
//        isBossFight = false;

//        // Reproducir diálogo de derrota del boss si existe
//        if (currentBossData.bossDefeatDialogue != null)
//        {
//            StartCoroutine(PlayBossDefeatDialogue());
//        }
//        else
//        {
//            CompleteBossFight();
//        }
//    }

//    private IEnumerator PlayBossDefeatDialogue()
//    {
//        GameFlowController.Instance?.TriggerDialogue(currentBossData.bossDefeatDialogue.dialogueId);

//        yield return null;

//        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
//        if (dialogueManager != null)
//        {
//            while (dialogueManager.IsDialoguePlaying)
//            {
//                yield return null;
//            }
//        }

//        CompleteBossFight();
//    }

//    private void CompleteBossFight()
//    {
//        GameEvents.OnBossDefeated?.Invoke();
//        GameEvents.OnAllEnemiesDefeated?.Invoke();
//    }

//    #endregion

//    #region Zone Restrictions

//    public bool IsPlayerInCombatZone(Vector3 playerPosition)
//    {
//        if (!isCombatActive) return true;

//        float distance = Vector3.Distance(playerPosition, combatCenter);
//        return distance <= combatRadius;
//    }

//    public bool CanPlayerLeaveZone()
//    {
//        // Solo puede salir si no hay enemigos activos
//        return activeEnemies.Count == 0;
//    }

//    #endregion

//    #region Cleanup

//    public void CleanupCombat()
//    {
//        // Destruir todos los enemigos activos
//        foreach (GameObject enemy in activeEnemies)
//        {
//            if (enemy != null)
//            {
//                Destroy(enemy);
//            }
//        }

//        activeEnemies.Clear();
//        isCombatActive = false;
//        isBossFight = false;
//        currentWaveIndex = 0;
//        currentCombatData = null;
//        currentBossData = null;
//    }

//    #endregion

//    #region Gizmos

//    private void OnDrawGizmos()
//    {
//        if (!showGizmos) return;

//        // Dibujar zona de combate
//        if (isCombatActive && combatRadius > 0)
//        {
//            Gizmos.color = combatZoneColor;
//            DrawWireCircle(combatCenter, combatRadius, 32);
//        }

//        // Dibujar posiciones de spawn si hay combat data configurado
//        if (currentCombatData != null)
//        {
//            Gizmos.color = Color.red;
//            foreach (var spawn in currentCombatData.enemySpawns)
//            {
//                Gizmos.DrawWireSphere(spawn.spawnPosition, 0.5f);
//            }
//        }
//    }

//    private void DrawWireCircle(Vector3 center, float radius, int segments)
//    {
//        float angleStep = 360f / segments;
//        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

//        for (int i = 1; i <= segments; i++)
//        {
//            float angle = i * angleStep * Mathf.Deg2Rad;
//            Vector3 newPoint = center + new Vector3(
//                Mathf.Cos(angle) * radius,
//                0,
//                Mathf.Sin(angle) * radius
//            );

//            Gizmos.DrawLine(previousPoint, newPoint);
//            previousPoint = newPoint;
//        }
//    }

//    #endregion
//}
