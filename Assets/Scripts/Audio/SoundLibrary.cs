using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Librería de sonidos organizada por ID de string.
/// Reemplaza los campos hardcodeados en AudioManager: en lugar de tener
/// 'public SoundData efecto_hover' directamente en el manager, creas un
/// asset de SoundLibrary para cada dominio (UI, música, combate, etc.)
/// y buscas los sonidos por ID desde el código.
///
/// DOMINIOS SUGERIDOS para Siecha:
///   - SoundLibrary_UI      → "ui_hover", "ui_confirm", "ui_pause", "ui_pickup"
///   - SoundLibrary_Music   → "music_menu", "music_combat", "music_boss", "music_death"
///   - SoundLibrary_Ambient → "amb_bosque", "amb_aldea", "amb_cueva"
///
/// Los sonidos de combate NO van aquí, van en RandomSoundSet dentro de los
/// datos de cada arma (ArmaData), porque son específicos de cada personaje/arma.
///
/// Crea un asset via: Audio > Sound Library
/// </summary>
[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("ID único para este sonido. Ejemplo: 'ui_hover', 'music_menu'")]
        public string id;
        public SoundData sound;
    }

    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    // Cache interno: se construye la primera vez que se consulta.
    // Esto resuelve el problema de búsqueda lineal del foreach original de VoiceLibrary.
    // Con Dictionary la búsqueda es O(1) sin importar cuántos sonidos tenga la librería.
    private Dictionary<string, SoundData> _cache;

    /// <summary>
    /// Devuelve el SoundData asociado al ID. Retorna null y loguea advertencia si no existe.
    /// </summary>
    public SoundData Get(string id)
    {
        BuildCacheIfNeeded();

        if (_cache.TryGetValue(id, out SoundData data))
            return data;

        Debug.LogWarning($"[SoundLibrary] '{name}': no se encontró el sonido con ID '{id}'.");
        return null;
    }

    /// <summary>
    /// Versión silenciosa: retorna null sin loguear si el ID no existe.
    /// Útil cuando un sonido es opcional y su ausencia no es un error.
    /// </summary>
    public SoundData TryGet(string id)
    {
        BuildCacheIfNeeded();
        _cache.TryGetValue(id, out SoundData data);
        return data;
    }

    // OnEnable se llama cuando Unity carga el ScriptableObject.
    // Construir el cache aquí garantiza que esté listo antes del primer Get().
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
        _cache = new Dictionary<string, SoundData>(sounds.Count);

        foreach (var entry in sounds)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                Debug.LogWarning($"[SoundLibrary] '{name}': una entrada tiene ID vacío, se omite.");
                continue;
            }
            if (_cache.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[SoundLibrary] '{name}': ID duplicado '{entry.id}', se usa el primero.");
                continue;
            }
            _cache[entry.id] = entry.sound;
        }
    }

    // Llamado automáticamente por Unity cuando modificas el asset en el Editor.
    // Reconstruye el cache para que los cambios se reflejen sin reiniciar.
    private void OnValidate()
    {
        BuildCache();
    }
}
