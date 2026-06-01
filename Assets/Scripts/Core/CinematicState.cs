using UnityEngine;

public class CinematicState : GameState
{
    private CinematicPlayer cinematicPlayer;
    private static bool isPaused = false;

    public CinematicState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        PausarJuego();
        cinematicPlayer = GameObject.FindObjectOfType<CinematicPlayer>();
        cinematicPlayer.OnCinematicFinished += OnCinematicEnd;
        cinematicPlayer.PlayCinematic(config.videoClip);

    }

    private void OnCinematicEnd()
    {
        manager.GoToNextSection();

    }
    public override void Update()
    {
        ControladorCambiarPersonaje.instance.OcultarTodosLosHUD();
    }
    public static void PausarJuego()
    {
        if (isPaused) return;
        Time.timeScale = 0f;
        isPaused = true;

    }

    /// <summary>
    /// Reanuda el juego después de una pausa o cinemática
    /// </summary>
    public static void ReanudarJuego()
    {
        if (!isPaused) return;
        Time.timeScale = 1f;
        isPaused = false;

    }

    public override void Exit()
    {
        if (cinematicPlayer != null)
            cinematicPlayer.OnCinematicFinished -= OnCinematicEnd;

        ControladorCambiarPersonaje.instance.ActivarHUDPausa();
        ReanudarJuego();
        ControladorCambiarPersonaje.instance.PuedePausar = true;
    }
}

