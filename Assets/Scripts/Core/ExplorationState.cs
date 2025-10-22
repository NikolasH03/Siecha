using UnityEngine;

public class ExplorationState : GameState
{
    public ExplorationState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        AudioManager.Instance.PlayMusic(config.musicData);
        AudioManager.Instance.PlayAmbience(config.ambienceData);
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
