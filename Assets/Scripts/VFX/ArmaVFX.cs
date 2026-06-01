using UnityEngine;

/// <summary>
/// Componente que vive en el prefab de cada arma y controla su trail visual.
/// Se referencia desde ControladorCombate para activar/desactivar el trail
/// durante los animation events de los ataques.
/// </summary>
public class ArmaVFX : MonoBehaviour
{
    [SerializeField] private GameObject trail;

    private void Awake()
    {
        // Advertencia temprana: si el trail no está asignado en el prefab del arma,
        // este log aparece al instanciar el arma — mucho mejor que un NullReferenceException
        // en medio del combate cuando ya es difícil rastrear el origen.
        if (trail == null)
        {
            Debug.LogWarning($"[ArmaVFX] '{gameObject.name}': el campo 'trail' no está asignado en el Inspector.");
        }
    }

    public void ActivarTrail()
    {
        if (trail == null) return;
        trail.SetActive(true);
    }

    public void DesactivarTrail()
    {
        if (trail == null) return;
        trail.SetActive(false);
    }
}
