using UnityEngine;

/// <summary>
/// ScriptableObject que agrupa variantes del mismo tipo de sonido.
/// Úsalo para pasos, golpes, bloqueos, cualquier sonido que necesite variedad.
/// Crea un asset via: Audio > Random Sound Set
/// </summary>
[CreateAssetMenu(menuName = "Audio/Random Sound Set")]
public class RandomSoundSet : ScriptableObject
{
    [Tooltip("Lista de variantes del mismo tipo de sonido (ataques, pasos, bloqueos...)")]
    public SoundData[] variations;

    // Guarda el índice del último sonido reproducido para evitar repetir el mismo dos veces seguidas.
    // Esto se llama "Last Played Avoidance" y hace que la variedad se sienta más natural.
    private int lastPlayedIndex = -1;

    public SoundData GetRandomSound()
    {
        if (variations == null || variations.Length == 0)
        {
            Debug.LogWarning($"[RandomSoundSet] '{name}' no tiene clips asignados.");
            return null;
        }

        // Con un solo sonido no hay elección posible
        if (variations.Length == 1)
            return variations[0];

        // Elegir un índice diferente al último para evitar repetición inmediata
        int index;
        do
        {
            index = Random.Range(0, variations.Length);
        }
        while (index == lastPlayedIndex);

        lastPlayedIndex = index;
        return variations[index];
    }
}