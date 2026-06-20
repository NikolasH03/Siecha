using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Componente en escena que agrupa los PlayableDirectors de un combate normal.
/// FightState lo busca en Enter() para obtener las referencias.
///
/// Cada escena de combate puede tener su propia cinemática de entrada y salida.
/// Dejar cualquier campo en null = modo placeholder (salta inmediatamente).
/// </summary>
public class BuscadorCinematicaCombate : MonoBehaviour
{
    [Header("Cinemáticas de este combate")]
    [Tooltip("Se reproduce al ENTRAR en el combate (inicio de FightState).")]
    [SerializeField] private PlayableDirector directorEntrada;

    [Tooltip("Se reproduce al SALIR del combate cuando el jugador activa " +
             "el CombatZoneTrigger. Asignarlo en el trigger, no aquí.")]
    [SerializeField] private PlayableDirector directorVictoria;

    public PlayableDirector DirectorEntrada  => directorEntrada;
    public PlayableDirector DirectorVictoria => directorVictoria;
}