using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class FightState : GameState
{
    private CombatZoneBarrier[] barriers;
    private bool combatEnded = false;

    // Director de victoria — se usa solo si el combate termina sin trigger
    // (todos los enemigos mueren Y no hay CombatZoneTrigger en escena).
    private PlayableDirector directorVictoria;

    public FightState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        Debug.Log("[FightState] Entrando.");

        if (config.musicData    != null) AudioManager.Instance.PlayMusic(config.musicData);
        if (config.ambienceData != null) AudioManager.Instance.PlayAmbience(config.ambienceData);

        barriers = Object.FindObjectsOfType<CombatZoneBarrier>();
        foreach (var b in barriers) b.SetBarrierActive(true);

        if (config.showTutorial)
            GameFlowManager.Instance.StartCoroutine(MostrarTutorialConRetraso(config.TutorialID, 2f));

        // Buscar cinemáticas de este combate en escena
        BuscadorCinematicaCombate buscador = Object.FindObjectOfType<BuscadorCinematicaCombate>();
        if (buscador != null) directorVictoria = buscador.DirectorVictoria;

        // Cinemática de ENTRADA al combate (puede ser null → placeholder)
        PlayableDirector directorEntrada = buscador?.DirectorEntrada;

        if (CinematicaManager.Instance != null)
        {
            // Reproducir intro y luego iniciar el loop de detección de fin
            CinematicaManager.Instance.Reproducir(
                directorEntrada,
                onTerminada: () => GameFlowManager.Instance.StartCoroutine(CheckCombatEndRoutine())
            );
        }
        else
        {
            GameFlowManager.Instance.StartCoroutine(CheckCombatEndRoutine());
        }
    }

    // ─── Detección de fin de combate ──────────────────────────────────────────

    private IEnumerator CheckCombatEndRoutine()
    {
        // Delay de seguridad para que los enemigos terminen de inicializarse
        yield return new WaitForSeconds(2f);

        while (!combatEnded)
        {
            if (EnemyManager.instance == null) yield break;

            if (EnemyManager.instance.AreAllEnemiesDead())
            {
                EndCombat();
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void EndCombat()
    {
        if (combatEnded) return;
        combatEnded = true;

        Debug.Log("[FightState] Todos los enemigos derrotados.");
        foreach (var b in barriers)
            if (b != null) b.SetBarrierActive(false);

        // Si hay CombatZoneTrigger en escena, él se encarga de avanzar
        // (el jugador decide cuándo activarlo). No avanzamos automáticamente.
        var trigger = Object.FindObjectOfType<CombatZoneTrigger>();
        if (trigger != null && trigger.gameObject.activeSelf)
        {
            Debug.Log("[FightState] Esperando que el jugador active el CombatZoneTrigger.");
            return;
        }

        // Sin trigger: avanzar directamente (con cinemática de victoria si la hay)
        if (CinematicaManager.Instance != null)
            CinematicaManager.Instance.Reproducir(directorVictoria, onTerminada: ContinuarFlujo);
        else
            ContinuarFlujo();
    }

    private void ContinuarFlujo()
    {
        var siguiente = GameFlowManager.Instance.GetNextSectionConfig();
        if (siguiente != null && siguiente.requiresSceneLoad)
        {
            Debug.Log("[FightState] Esperando trigger de cambio de escena.");
            return;
        }
        GameFlowManager.Instance.GoToNextSection();
    }

    private IEnumerator MostrarTutorialConRetraso(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        MenuManager.Instance?.AbrirPanelTutorial(index);
    }

    public override void Update() { }

    public override void Exit()
    {
        if (barriers != null)
            foreach (var b in barriers)
                if (b != null) b.SetBarrierActive(false);

        CinematicaManager.Instance?.ForzarDetener();
    }
}