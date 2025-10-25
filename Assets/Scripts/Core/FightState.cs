using UnityEngine;
using System.Collections;

public class FightState : GameState
{
    private CombatZoneBarrier[] barriers;
    private bool combatEnded = false;

    public FightState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        Debug.Log("[CombatState] Entrando al estado de combate");

        if (config.musicData != null)
            AudioManager.Instance.PlayMusic(config.musicData);

        if (config.ambienceData != null)
            AudioManager.Instance.PlayAmbience(config.ambienceData);

        barriers = Object.FindObjectsOfType<CombatZoneBarrier>();

        foreach (var barrier in barriers)
        {
            barrier.SetBarrierActive(true);
            Debug.Log($"[CombatState] Activando barrera: {barrier.name}");
        }

        GameFlowManager.Instance.StartCoroutine(CheckCombatEndRoutine());
    }

    private IEnumerator CheckCombatEndRoutine()
    {
        yield return new WaitForSeconds(2f); // pequeño delay de seguridad

        while (!combatEnded)
        {
            if (EnemyManager.instance == null)
                yield break;

            bool allDead = EnemyManager.instance.AreAllEnemiesDead();

            if (allDead)
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
        Debug.Log("[CombatState] Todos los enemigos han sido derrotados.");

        foreach (var barrier in barriers)
            barrier.SetBarrierActive(false);

        var nextConfig = GameFlowManager.Instance.GetNextSectionConfig();
        if (nextConfig != null && nextConfig.requiresSceneLoad)
        {
            Debug.Log("[CombatState] Esperando a que el jugador active el trigger de cambio de escena...");
            return;
        }

        // Si no requiere cambio de escena, continuar normalmente
        GameFlowManager.Instance.GoToNextSection();
    }

    public override void Exit()
    {
        if (barriers != null)
        {
            foreach (var barrier in barriers)
                barrier.SetBarrierActive(false);
        }
    }

    public override void Update() { }
}
