using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [SerializeField] private VoiceLibrary protagonistVoices;
    [SerializeField] private VoiceLibrary enemyVoices;
    [SerializeField] private VoiceLibrary bossVoices;

    private bool isPlaying;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayRandomDialogue(string id, bool isEnemy = false, float duration = 3f)
    {
        if (isPlaying) return;

        isPlaying = true;

        // Elegir la librería según el tipo de personaje
        VoiceLibrary library = isEnemy ? enemyVoices : protagonistVoices;
        SoundData voiceClip = library.GetRandomVoice(id);

        if (voiceClip != null)
        {
            AudioManager.Instance.PlayVoiceLine(voiceClip);
        }

        Invoke(nameof(EndDialogue), duration);
    }

    private void EndDialogue()
    {
        isPlaying = false;
    }
}
