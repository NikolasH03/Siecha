using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Componente opcional en escenas de combate normal.
/// Si existe, FightState reproduce la cinemática de victoria al terminar.
/// Si no existe (o DirectorVictoria es null), avanza sin cinemática.
/// </summary>
public class BuscadorCinematicaCombate : MonoBehaviour
{
    [Tooltip("Cinemática que se reproduce al derrotar a todos los enemigos. " +
             "Dejar null = avanzar directamente sin cinemática.")]
    [SerializeField] private PlayableDirector directorVictoria;

    public PlayableDirector DirectorVictoria => directorVictoria;
}
