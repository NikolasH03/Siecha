using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Componente en escena que agrupa las referencias a los PlayableDirectors
/// del jefe. BossBattleState lo busca en Enter() para obtener los directores.
///
/// Separar las referencias de la lógica permite:
/// - Arte asignar los directores sin tocar código de estado
/// - Probar sin cinemáticas (dejar campos en null = placeholder)
/// - Reutilizar BossBattleState en diferentes escenas con diferentes clips
/// </summary>
public class BuscadorCinematicasJefe : MonoBehaviour
{
    [Header("Directores de Cinemáticas del Jefe")]
    [Tooltip("Dejar null = placeholder (salta inmediatamente).")]
    [SerializeField] private PlayableDirector intro;
    [SerializeField] private PlayableDirector transicionFase2;
    [SerializeField] private PlayableDirector transicionFase3;
    [SerializeField] private PlayableDirector muerte;

    public BossBattleState.ConfigCinematicasJefe ObtenerConfiguracion()
    {
        return new BossBattleState.ConfigCinematicasJefe
        {
            intro           = intro,
            transicionFase2 = transicionFase2,
            transicionFase3 = transicionFase3,
            muerte          = muerte
        };
    }
}
