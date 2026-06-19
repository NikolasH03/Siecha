using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Trigger que el jugador activa para avanzar a la siguiente sección.
///
/// FLUJO CON CINEMÁTICA:
///   Jugador entra → cinemática de salida → GoToNextSection() → nueva escena
///   → FightState.Enter() reproduce cinemática de entrada → comienza combate
///
/// FLUJO SIN CINEMÁTICA (director null):
///   Jugador entra → GoToNextSection() directamente
///
/// Separar el trigger de la lógica de cinemáticas permite reutilizarlo
/// en cualquier tipo de sección (exploración, combate, boss).
/// </summary>
public class CombatZoneTrigger : MonoBehaviour
{
    [Tooltip("Cinemática que se reproduce ANTES de cambiar de sección/escena. " +
             "Dejar null para avanzar directamente sin cinemática.")]
    [SerializeField] private PlayableDirector directorSalida;

    [Tooltip("Si está activo, solo el jugador puede activar este trigger.")]
    [SerializeField] private bool soloJugador = true;

    private GameObject player;
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (soloJugador && !other.CompareTag("Player")) return;

        player=other.gameObject;
        player.SetActive(false);
        activado = true;
        gameObject.SetActive(false);

        if (directorSalida != null && CinematicaManager.Instance != null)
        {
            // Reproducir cinemática de salida, luego avanzar
            CinematicaManager.Instance.Reproducir(directorSalida, onTerminada: Avanzar);
        }
        else
        {
            // Sin cinemática: avanzar directamente
            player.gameObject.SetActive(true);
            Avanzar();
        }
    }

    private void Avanzar()
    {
        player.gameObject.SetActive(true);
        GameFlowManager.Instance.GoToNextSection();
    }
}