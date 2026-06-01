using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

/// <summary>
/// Estado del GameFlowManager durante la pelea del jefe.
/// Usa CinematicaManager en lugar de CinematicaJefe — el manager
/// centralizado se encarga del input y del PlayableDirector.
/// </summary>
public class BossBattleState : GameState
{
    private CombatZoneBarrier[] barriers;
    private BossEnemigo         boss;
    private bool                combatEnded = false;

    [System.Serializable]
    public class ConfigCinematicasJefe
    {
        public PlayableDirector intro;
        public PlayableDirector transicionFase2;
        public PlayableDirector transicionFase3;
        public PlayableDirector muerte;
    }

    // Las referencias a los directores vienen del SectionConfig o se buscan en escena.
    // Diseño: BossBattleState no conoce CinematicaJefe, solo entrega PlayableDirectors
    // a CinematicaManager. Así cualquier estado puede usar el mismo patrón.
    private ConfigCinematicasJefe cinematicas;

    public BossBattleState(GameFlowManager manager, SectionConfig config)
        : base(manager, config) { }

    // ─── Enter ────────────────────────────────────────────────────────────────

    public override void Enter()
    {
        Debug.Log("[BossBattleState] Entrando.");

        if (config.musicData    != null) AudioManager.Instance.PlayMusic(config.musicData);
        if (config.ambienceData != null) AudioManager.Instance.PlayAmbience(config.ambienceData);

        barriers = Object.FindObjectsOfType<CombatZoneBarrier>();
        foreach (var b in barriers) b.SetBarrierActive(true);

        if (config.showTutorial)
            GameFlowManager.Instance.StartCoroutine(MostrarTutorialConRetraso(config.TutorialID, 2f));

        SuscribirseAEventos();

        boss = Object.FindObjectOfType<BossEnemigo>();
        if (boss == null)
            Debug.LogWarning("[BossBattleState] No se encontró BossEnemigo.");

        // Buscar los directores de cinemáticas en escena
        // (o null si arte aún no los ha entregado → placeholder automático)
        BuscadorCinematicasJefe buscador = Object.FindObjectOfType<BuscadorCinematicasJefe>();
        if (buscador != null) cinematicas = buscador.ObtenerConfiguracion();

        // Cinemática de intro
        ReproducirCinematica(
            cinematicas?.intro,
            onTerminada: () => Debug.Log("[BossBattleState] Intro lista. Combate activo.")
        );
    }

    // ─── Eventos ──────────────────────────────────────────────────────────────

    private void SuscribirseAEventos()
    {
        BossEnemigo.OnCambioFase           += HandleCambioFase;
        BossEstadoMuerte.OnBossMuertoListo += HandleBossMuerto;
    }

    private void DesuscribirseDeEventos()
    {
        BossEnemigo.OnCambioFase           -= HandleCambioFase;
        BossEstadoMuerte.OnBossMuertoListo -= HandleBossMuerto;
    }

    private void HandleCambioFase(BossEnemigo jefe, int nuevaFase)
    {
        PlayableDirector director = nuevaFase == 2
            ? cinematicas?.transicionFase2
            : cinematicas?.transicionFase3;

        ReproducirCinematica(director, onTerminada: () => jefe.ReanudarDesdeFase(nuevaFase));
    }

    private void HandleBossMuerto(BossEnemigo jefe)
    {
        if (combatEnded) return;
        combatEnded = true;

        ReproducirCinematica(
            cinematicas?.muerte,
            onTerminada: () =>
            {
                jefe.GetHealthComp().Eliminar();
                DesactivarBarreras();
                AvanzarSiguienteSeccion();
            }
        );
    }

    // ─── Cinematicas ──────────────────────────────────────────────────────────

    private void ReproducirCinematica(PlayableDirector director, System.Action onTerminada)
    {
        if (CinematicaManager.Instance != null)
            CinematicaManager.Instance.Reproducir(director, onTerminada);
        else
            onTerminada?.Invoke(); // fallback si no hay manager
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void DesactivarBarreras()
    {
        if (barriers == null) return;
        foreach (var b in barriers) b.SetBarrierActive(false);
    }

    private void AvanzarSiguienteSeccion()
    {
        var siguiente = GameFlowManager.Instance.GetNextSectionConfig();
        if (siguiente != null && siguiente.requiresSceneLoad)
        {
            Debug.Log("[BossBattleState] Esperando trigger del jugador.");
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
        DesuscribirseDeEventos();
        CinematicaManager.Instance?.ForzarDetener();
        DesactivarBarreras();
    }
}