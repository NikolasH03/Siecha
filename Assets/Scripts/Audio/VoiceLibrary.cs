using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Librería de líneas de voz organizadas por ID.
/// Cada ID puede tener múltiples variantes; se elige una aleatoria con
/// Last Played Avoidance (igual que RandomSoundSet) para que no se repita
/// la misma línea dos veces seguidas.
///
/// IDs sugeridos para Siecha:
///   "Muisca_Muerte", "Muisca_Golpe", "Muisca_Combo", "Muisca_BajaVida"
///   "Espanol_Muerte", "Espanol_Golpe", "Espanol_Combo", "Espanol_BajaVida"
///   "Enemy_Taunt", "Enemy_Alerta", "Enemy_Muerte"
///
/// Crea un asset via: Audio > Voice Library
/// </summary>
[CreateAssetMenu(menuName = "Audio/Voice Library")]
public class VoiceLibrary : ScriptableObject
{
    [System.Serializable]
    public class VoiceEntry
    {
        [Tooltip("ID único para este grupo de líneas. Ejemplo: 'Muisca_Muerte'")]
        public string id;

        [Tooltip("Variantes de esa línea. Se elige una aleatoria cada vez.")]
        public SoundData[] variations;
    }

    [SerializeField] private List<VoiceEntry> voices = new List<VoiceEntry>();

    // Mismo patrón de cache con Dictionary que SoundLibrary.
    // El foreach original recorría toda la lista en cada llamada — con un
    // diccionario la búsqueda es instantánea sin importar cuántas entradas haya.
    private Dictionary<string, VoiceEntry> _cache;

    // Guarda el último índice reproducido por ID para evitar repetición inmediata.
    private Dictionary<string, int> _lastPlayedIndex = new Dictionary<string, int>();

    /// <summary>
    /// Devuelve una SoundData aleatoria para el ID dado, evitando repetir la última.
    /// </summary>
    public SoundData GetRandomVoice(string id)
    {
        BuildCacheIfNeeded();

        if (!_cache.TryGetValue(id, out VoiceEntry entry))
        {
            Debug.LogWarning($"[VoiceLibrary] '{name}': no se encontró la voz con ID '{id}'.");
            return null;
        }

        if (entry.variations == null || entry.variations.Length == 0)
        {
            Debug.LogWarning($"[VoiceLibrary] '{name}': la entrada '{id}' no tiene variantes asignadas.");
            return null;
        }

        if (entry.variations.Length == 1)
            return entry.variations[0];

        // Last Played Avoidance
        _lastPlayedIndex.TryGetValue(id, out int lastIndex);
        int newIndex;
        do
        {
            newIndex = Random.Range(0, entry.variations.Length);
        }
        while (newIndex == lastIndex);

        _lastPlayedIndex[id] = newIndex;
        return entry.variations[newIndex];
    }

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCacheIfNeeded()
    {
        if (_cache == null) BuildCache();
    }

    private void BuildCache()
    {
        _cache = new Dictionary<string, VoiceEntry>(voices.Count);

        foreach (var entry in voices)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                Debug.LogWarning($"[VoiceLibrary] '{name}': una entrada tiene ID vacío, se omite.");
                continue;
            }
            if (_cache.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[VoiceLibrary] '{name}': ID duplicado '{entry.id}', se usa el primero.");
                continue;
            }
            _cache[entry.id] = entry;
        }
    }

    private void OnValidate()
    {
        BuildCache();
    }
}