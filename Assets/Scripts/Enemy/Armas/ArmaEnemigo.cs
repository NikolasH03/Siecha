using UnityEngine;

/// <summary>Collider del arma melee del enemigo: al tocar al jugador le aplica el daño del ataque actual.</summary>
public class ArmaEnemigo : MonoBehaviour
{
    [SerializeField] private Enemigo enemigo;

    private void Awake()
    {
        if (enemigo == null) enemigo = GetComponentInParent<Enemigo>();

        if (enemigo == null)
            Debug.LogError($"[{name}] ArmaEnemigo no encontró un Enemigo en sus padres.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemigo == null || !other.CompareTag("Player")) return;

        ControladorCombate jugador = other.GetComponent<ControladorCombate>();
        if (jugador != null)
            jugador.JugadorRecibeDano(enemigo.ObtenerDanoActual());
    }
}