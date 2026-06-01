using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class FightState : GameState
{
    private CombatZoneBarrier[] barriers;
    private bool combatEnded = false;

    // Director de cinemática de victoria (opcional).
    // Si está en null → placeholder, avanza inmediatamente.
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

        // Buscar cinemática de victoria de este combate en escena (opcional)
        BuscadorCinematicaCombate buscador = Object.FindObjectOfType<BuscadorCinematicaCombate>();
        if (buscador != null) directorVictoria = buscador.DirectorVictoria;

        GameFlowManager.Instance.StartCoroutine(CheckCombatEndRoutine());
    }

    private IEnumerator CheckCombatEndRoutine()
    {
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
        foreach (var b in barriers) b.SetBarrierActive(false);

        // Cinemática de victoria (puede ser null → placeholder)
        if (CinematicaManager.Instance != null)
        {
            CinematicaManager.Instance.Reproducir(directorVictoria, onTerminada: ContinuarFlujo);
        }
        else
        {
            ContinuarFlujo();
        }
    }

    private void ContinuarFlujo()
    {
        var siguiente = GameFlowManager.Instance.GetNextSectionConfig();
        if (siguiente != null && siguiente.requiresSceneLoad)
        {
            Debug.Log("[FightState] Esperando trigger del jugador para cambiar escena.");
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
            foreach (var b in barriers) b.SetBarrierActive(false);

        CinematicaManager.Instance?.ForzarDetener();
    }
}