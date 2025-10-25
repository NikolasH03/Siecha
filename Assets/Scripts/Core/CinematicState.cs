using UnityEngine;

public class CinematicState : GameState
{
    private CinematicPlayer cinematicPlayer;

    public CinematicState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        //ControladorCambiarPersonaje.instance.OcultarTodosLosHUD();
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
    }

    public override void Exit()
    {
        if (cinematicPlayer != null)
            cinematicPlayer.OnCinematicFinished -= OnCinematicEnd;

        //ControladorCambiarPersonaje.instance.ActivarHUDPausa();
    }
}

