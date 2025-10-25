using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CombatZoneBarrier : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool activeOnStart = false;
    [SerializeField] private string playerTag = "Player";

    [Header("Feedback Visual (Opcional)")]
    [SerializeField] private ParticleSystem blockEffectPrefab;
    [SerializeField] private float effectDuration = 0.5f;

    private Collider barrierCollider;
    private bool isActive;
    private float lastBlockTime;
    private const float blockCooldown = 1f;

    private void Awake()
    {
        barrierCollider = GetComponent<Collider>();
        // Arranca como trigger para evitar bloquear antes de tiempo
        barrierCollider.isTrigger = true;
        barrierCollider.enabled = activeOnStart;
        isActive = activeOnStart;
    }

    public void SetBarrierActive(bool active)
    {
        isActive = active;

        // Si está activa ? no es trigger ? bloquea físicamente
        // Si está inactiva ? vuelve a trigger y se desactiva
        barrierCollider.isTrigger = !active;
        barrierCollider.enabled = active;

        if (active)
        {
            Debug.Log($"[Barrier] Activada en {gameObject.name}");
        }
        else
        {
            Debug.Log($"[Barrier] Desactivada en {gameObject.name}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        if (collision.collider.CompareTag(playerTag))
        {
            if (Time.time - lastBlockTime > blockCooldown)
            {
                lastBlockTime = Time.time;
                PlayBlockEffects(collision.contacts[0].point);
            }
        }
    }

    private void PlayBlockEffects(Vector3 position)
    {
        if (blockEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(blockEffectPrefab, position, Quaternion.identity);
            Destroy(effect.gameObject, effectDuration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? new Color(1f, 0f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.2f);
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawWireCube(box.center, box.size);
        else if (col is SphereCollider sphere)
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
    }
}
