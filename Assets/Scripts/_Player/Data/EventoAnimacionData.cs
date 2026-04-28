using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define TODOS los eventos de animación de un personaje o enemigo.
/// Cada evento tiene un ID legible, sonido, VFX y el nombre del pivot desde donde se reproduce.
///
/// CREA UN ASSET POR PERSONAJE/ENEMIGO:
///   Clic derecho en Project → Audio/Eventos → Evento Animacion Data
///   Nombres sugeridos:
///     EventoAnim_Muisca
///     EventoAnim_Espanol
///     EventoAnim_EnemBase
///     EventoAnim_EnemDistancia
///     EventoAnim_EnemTanque
///     EventoAnim_EnemPicaro
///     EventoAnim_JefeFase1
///     EventoAnim_JefeFase2
///     EventoAnim_JefeFase3
/// </summary>
[CreateAssetMenu(menuName = "Audio/Eventos/Evento Animacion Data")]
public class EventoAnimacionData : ScriptableObject
{
    [System.Serializable]
    public class EventoEntry
    {
        [Tooltip("Nombre único del evento. Este mismo string va en el Animation Event en Unity.\n" +
                 "Ejemplos: 'golpe_macana', 'paso_izq_roca', 'disparo_arcabuz'")]
        public string id;

        [Space(4)]
        [Tooltip("Sonido único (no aleatorio). Déjalo vacío si usas sonidoAleatorio.")]
        public SoundData sonido;

        [Tooltip("Sonido aleatorio entre variantes. Tiene prioridad sobre 'sonido' si ambos están asignados.")]
        public RandomSoundSet sonidoAleatorio;

        [Space(4)]
        [Tooltip("Efecto visual. Déjalo vacío si el evento no tiene VFX.")]
        public VFXData vfx;

        [Space(4)]
        [Tooltip("Nombre del pivot desde donde se reproduce el efecto.\n" +
                 "Debe coincidir con un ID en la lista 'pivotsImpacto' del EventosAnimacion.\n" +
                 "Déjalo vacío para usar la posición raíz del personaje.")]
        public string pivotId;
    }

    [SerializeField] private List<EventoEntry> eventos = new List<EventoEntry>();

    // Cache interno — búsqueda O(1) por string en lugar de O(n) con foreach
    private Dictionary<string, EventoEntry> _cache;

    // ════════════════════════════════════════════════════════════════════════
    // API PÚBLICA
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Busca un evento por ID. Devuelve null y loguea si no existe.
    /// </summary>
    public EventoEntry GetEvento(string id)
    {
        BuildCacheIfNeeded();

        if (_cache.TryGetValue(id, out EventoEntry entry))
            return entry;

        Debug.LogWarning($"[EventoAnimacionData] '{name}': no se encontró el evento '{id}'.\n" +
                         "Verifica que el ID en el Animation Event coincide exactamente con el del asset.");
        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CACHE
    // ════════════════════════════════════════════════════════════════════════

    private void OnEnable() => BuildCache();
    private void OnValidate() => BuildCache();

    private void BuildCacheIfNeeded()
    {
        if (_cache == null) BuildCache();
    }

    private void BuildCache()
    {
        _cache = new Dictionary<string, EventoEntry>(eventos.Count);

        foreach (var entry in eventos)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                Debug.LogWarning($"[EventoAnimacionData] '{name}': una entrada tiene ID vacío, se omite.");
                continue;
            }
            if (_cache.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[EventoAnimacionData] '{name}': ID duplicado '{entry.id}', se usa el primero.");
                continue;
            }
            _cache[entry.id] = entry;
        }
    }
}