using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsState : GameState
{
    public CreditsState(GameFlowManager manager) : base(manager) { }

    public override void Enter()
    {
        SceneManager.LoadScene("CreditsScene");
    }

    public override void Update() { }
    public override void Exit() { }
}
