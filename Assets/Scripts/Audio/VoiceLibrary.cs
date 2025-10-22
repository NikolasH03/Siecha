using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/Voice Library")]
public class VoiceLibrary : ScriptableObject
{
    [System.Serializable]
    public class VoiceEntry
    {
        public string id; // Ejemplo: "Prota_Muerte", "Enemy_Taunt"
        public SoundData[] variations; // Varias posibles líneas de voz para ese ID
    }

    [SerializeField] private List<VoiceEntry> voices = new List<VoiceEntry>();

    public SoundData GetRandomVoice(string id)
    {
        foreach (var entry in voices)
        {
            if (entry.id == id && entry.variations.Length > 0)
            {
                int randomIndex = Random.Range(0, entry.variations.Length);
                return entry.variations[randomIndex];
            }
        }

        Debug.LogWarning("No se encontró la voz con ID: " + id);
        return null;
    }
}
