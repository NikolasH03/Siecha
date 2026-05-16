using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que vive en cada personaje/enemigo y ejecuta sus eventos de animación.
/// Conecta el Animator con AudioManager y VFXPool usando nombres legibles en lugar de índices.
///
/// USO EN EL ANIMATOR:
///   En cada Animation Event, llama al método 'Reproducir' con el string del evento.
///   Ejemplo: Function = "Reproducir", String = "golpe_macana"
///
/// </summary>
public class EventosAnimacion : MonoBehaviour
{
    [Header("Datos del personaje")]
    [Tooltip("Asset con todos los eventos de este personaje. " +
             "Cada personaje y tipo de enemigo tiene el suyo propio.")]
    [SerializeField] private EventoAnimacionData datosEventos;

    [Header("Pivots de impacto")]
    [Tooltip("Puntos del esqueleto desde donde salen los efectos. " +
             "El ID debe coincidir exactamente con el 'pivotId' definido en el EventoAnimacionData.")]
    [SerializeField] private List<PivotNombrado> pivotsImpacto;

    // Cache de pivots por nombre — igual que el resto del sistema, O(1) en lugar de O(n)
    private Dictionary<string, Transform> _pivotCache;

    // ════════════════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        BuildPivotCache();

        if (datosEventos == null)
            Debug.LogWarning($"[EventosAnimacion] '{gameObject.name}': 'datosEventos' no está asignado.");
    }

    private void BuildPivotCache()
    {
        _pivotCache = new Dictionary<string, Transform>(pivotsImpacto.Count);

        foreach (var pivot in pivotsImpacto)
        {
            if (string.IsNullOrEmpty(pivot.id))
            {
                Debug.LogWarning($"[EventosAnimacion] '{gameObject.name}': un pivot tiene ID vacío, se omite.");
                continue;
            }
            if (pivot.transform == null)
            {
                Debug.LogWarning($"[EventosAnimacion] '{gameObject.name}': el pivot '{pivot.id}' no tiene Transform asignado.");
                continue;
            }
            if (_pivotCache.ContainsKey(pivot.id))
            {
                Debug.LogWarning($"[EventosAnimacion] '{gameObject.name}': ID de pivot duplicado '{pivot.id}', se usa el primero.");
                continue;
            }
            _pivotCache[pivot.id] = pivot.transform;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // API PRINCIPAL — este es el único método que llaman los Animation Events
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Punto de entrada único para todos los Animation Events.
    /// Busca el evento por nombre en el asset del personaje y ejecuta
    /// su sonido y VFX automáticamente desde el pivot correcto.
    ///
    /// En el Animator: Function = "Reproducir", String = "nombre_del_evento"
    /// </summary>
    public void Reproducir(string idEvento)
    {
        if (datosEventos == null) return;

        EventoAnimacionData.EventoEntry evento = datosEventos.GetEvento(idEvento);
        if (evento == null) return;

        Vector3 posicion = ObtenerPosicion(evento.pivotId);
        Quaternion rotacion = ObtenerRotacion(evento.pivotId);

        // Sonido: el aleatorio tiene prioridad sobre el único si ambos están asignados
        if (evento.sonidoAleatorio != null)
            AudioManager.Instance.PlayRandomSFX(evento.sonidoAleatorio, posicion);
        else if (evento.sonido != null)
            AudioManager.Instance.PlaySFX(evento.sonido, posicion);

        // VFX: solo si está asignado
        if (evento.vfx != null)
            VFXPool.Instance.PlayVFX(evento.vfx, posicion, rotacion);
    }
    public void ReproducirTransform(string idEvento, GameObject posicionEvento)
    {
        if (datosEventos == null) return;

        EventoAnimacionData.EventoEntry evento = datosEventos.GetEvento(idEvento);
        if (evento == null) return;

        Vector3 posicion = posicionEvento.transform.position;
        Quaternion rotacion = posicionEvento.transform.rotation;

        // Sonido: el aleatorio tiene prioridad sobre el único si ambos están asignados
        if (evento.sonidoAleatorio != null)
            AudioManager.Instance.PlayRandomSFX(evento.sonidoAleatorio, posicion);
        else if (evento.sonido != null)
            AudioManager.Instance.PlaySFX(evento.sonido, posicion);

        // VFX: solo si está asignado
        if (evento.vfx != null)
            VFXPool.Instance.PlayVFX(evento.vfx, posicion, rotacion);
    }

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS INTERNOS
    // ════════════════════════════════════════════════════════════════════════

    private Vector3 ObtenerPosicion(string pivotId)
    {
        if (!string.IsNullOrEmpty(pivotId) && _pivotCache.TryGetValue(pivotId, out Transform t))
            return t.position;

        return transform.position;
    }

    private Quaternion ObtenerRotacion(string pivotId)
    {
        if (!string.IsNullOrEmpty(pivotId) && _pivotCache.TryGetValue(pivotId, out Transform t))
            return t.rotation;

        return transform.rotation;
    }
}

/// <summary>
/// Par ID + Transform para definir pivots de impacto en el Inspector.
/// </summary>
[System.Serializable]
public class PivotNombrado
{
    [Tooltip("Nombre del pivot. Debe coincidir con el 'pivotId' en EventoAnimacionData.")]
    public string id;
    public Transform transform;
}


