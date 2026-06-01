using UnityEngine;
using System.Collections;
public class ExplorationState : GameState
{
    public ExplorationState(GameFlowManager manager, SectionConfig config) : base(manager, config) { }

    public override void Enter()
    {
        AudioManager.Instance.PlayMusic(config.musicData);
        AudioManager.Instance.PlayAmbience(config.ambienceData);

        if (config.showTutorial)
            GameFlowManager.Instance.StartCoroutine(MostrarTutorialConRetraso(config.TutorialID, 2f));
    }
    private IEnumerator MostrarTutorialConRetraso(int tutorialIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        MenuManager.Instance?.AbrirPanelTutorial(tutorialIndex);
    }
    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
