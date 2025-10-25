using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class GameStage
{
    public string sceneName;
    public bool isCombatSection;
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private GameState currentState;

    [Header("Managers")]
    public DialogueManager dialogueManager;
    public TutorialManager tutorialManager;

    [Header("Game Sections")]
    public SectionConfig[] sections; 
    private int currentSectionIndex = 0;

    public bool startGameplay = false;

private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.OnSceneLoadedSuccessfully += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.OnSceneLoadedSuccessfully -= OnSceneLoaded;
    }
    private void OnSceneLoaded(string sceneName)
    {
        if (startGameplay)
        {
            startGameplay = false;
            Debug.Log("arranca el flujo de juego");
            if (sections.Length > 0)
                ChangeState(CreateStateFromConfig(sections[currentSectionIndex]));
        }
    }

    public void StartGameplayFlow()
    {
        startGameplay = true;
    }

    private void Update()
    {
        currentState?.Update();

        //Debug.Log("estado actual: " + currentState);
    }

    public void ChangeState(GameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void GoToNextSection()
    {
        currentSectionIndex++;

        if (currentSectionIndex >= sections.Length)
        {
            Debug.Log("Juego completado. Ir a créditos.");
            ChangeState(new CreditsState(this));
            return;
        }

        SectionConfig nextConfig = sections[currentSectionIndex];

        if (nextConfig.requiresSceneLoad)
        {
            Debug.Log($"[GameFlowManager] Cargando nueva escena: {nextConfig.sceneName}");

            // Activa la bandera para que cuando la escena termine de cargarse,
            // se reinicie el flujo desde la nueva escena
            startGameplay = true;

            // Busca el ID de la escena en la lista de SceneLoader
            int sceneIndex = SceneLoader.Instance.NombresEscenas.IndexOf(nextConfig.sceneName) + 1;
            if (sceneIndex > 0)
            {
                SceneLoader.Instance.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError($"[GameFlowManager] La escena '{nextConfig.sceneName}' no está en la lista de SceneLoader.");
            }

            return; // No cambia de estado todavía
        }

        //Si no requiere carga, continúa al siguiente estado directamente
        ChangeState(CreateStateFromConfig(nextConfig));
    }

    public SectionConfig GetNextSectionConfig()
    {
        int nextIndex = currentSectionIndex + 1;
        if (nextIndex < sections.Length)
            return sections[nextIndex];
        return null;
    }

    private GameState CreateStateFromConfig(SectionConfig config)
    {
        switch (config.type)
        {
            case SectionType.Cinematic: return new CinematicState(this, config);
            case SectionType.Exploration: return new ExplorationState(this, config);
            case SectionType.Combat: return new FightState(this, config);
            case SectionType.BossBattle: return new BossBattleState(this, config);
            default: return new ExplorationState(this, config);
        }
    }
}
