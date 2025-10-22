using UnityEngine;

public class BossBattleState : GameState
{
    public BossBattleState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.mus_bossFight);
        //manager.dialogueManager.PlayDialogue("BossIntro");
        //BossController.Instance?.StartBattle(); INICIAR PELEA CON JEFE
    }

    public override void Update()
    {
        //if (BossController.Instance != null && BossController.Instance.Defeated)
        //{
        //    manager.GoToNextSection();
        //}
    }

    public override void Exit()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.mus_EndBossFight);
    }
}
