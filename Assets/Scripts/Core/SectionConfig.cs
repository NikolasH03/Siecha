using UnityEngine;
using UnityEngine.Video;

public enum SectionType { Cinematic, Exploration, Combat, BossBattle }

[System.Serializable]
public class SectionConfig
{
    public SectionType type;
    public VideoClip videoClip;
    public bool showTutorial;
    public int TutorialID;

    [Header("Audio")]
    public SoundData musicData;
    public SoundData ambienceData;

    [Header("Cambio de Escena")]
    public bool requiresSceneLoad;
    public string sceneName;
}
