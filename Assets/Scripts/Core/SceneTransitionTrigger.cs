using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Si está marcado, al tocar el trigger cargará la siguiente sección en el flujo.")]
    [SerializeField] private bool triggerNextSection = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;
        Debug.Log("[SceneTransitionTrigger] Jugador activó transición de sección.");

        if (triggerNextSection)
        {
            GameFlowManager.Instance.GoToNextSection();
        }
    }
}
