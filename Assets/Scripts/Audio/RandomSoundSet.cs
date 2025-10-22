using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Random Sound Set")]
public class RandomSoundSet : ScriptableObject
{
    [Tooltip("Lista de variantes del mismo tipo de sonido (ataques, pasos, bloqueos...)")]
    public SoundData[] variations;

    public SoundData GetRandomSound()
    {
        if (variations == null || variations.Length == 0)
        {
            Debug.LogWarning("RandomSoundSet sin clips asignados.");
            return null;
        }

        int index = Random.Range(0, variations.Length);
        return variations[index];
    }
}
