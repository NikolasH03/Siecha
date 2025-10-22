using UnityEngine;

public abstract class GameState
{
    protected GameFlowManager manager;
    protected SectionConfig config;

    public GameState(GameFlowManager manager, SectionConfig config = null)
    {
        this.manager = manager;
        this.config = config;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
