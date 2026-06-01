using UnityEngine;

/// <summary>
/// SO de configuración del jefe. Contiene un EnemyStats por fase y
/// los umbrales de vida que disparan las transiciones.
///
/// SETUP en el Inspector:
/// - Fase Uno stats: cuchillos — puedeBloquear=false, puedeEsquivar=true, agresividad=1.2
/// - Fase Dos stats: lanza+escudo — puedeBloquear=true, puedeEsquivar=false, agresividad=0.8
/// - Fase Tres stats: macana — puedeBloquear=false, puedeEsquivar=true, agresividad=0.5
/// </summary>
[CreateAssetMenu(fileName = "NewBossStats", menuName = "Combat/Boss Stats")]
public class BossStats : ScriptableObject
{
    [Header("Stats por Fase")]
    [Tooltip("Fase 1: doble cuchillo. Rapido, solo esquiva, no bloquea.")]
    public EnemyStats statsFaseUno;

    [Tooltip("Fase 2: lanza + escudo. Bloquea, no esquiva, mas lento.")]
    public EnemyStats statsFaseDos;

    [Tooltip("Fase 3: macana. Modo berserker. multiplicadorAgresividad bajo = ataca muy frecuente.")]
    public EnemyStats statsFaseTres;

    [Header("Umbrales de Transicion (% de vida total)")]
    [Tooltip("Al llegar a este % de vida, transiciona a fase 2.")]
    [Range(0f, 1f)] public float umbralFaseDos  = 0.66f;

    [Tooltip("Al llegar a este % de vida, transiciona a fase 3.")]
    [Range(0f, 1f)] public float umbralFaseTres = 0.33f;

    [Header("Armas Visuales por Fase")]
    [Tooltip("GameObject con los meshes/colisores de la fase 1. Se activa al iniciar.")]
    public GameObject prefabArmaFaseUno;
    [Tooltip("GameObject con los meshes/colisores de la fase 2.")]
    public GameObject prefabArmaFaseDos;
    [Tooltip("GameObject con los meshes/colisores de la fase 3.")]
    public GameObject prefabArmaFaseTres;

    // Helper para obtener stats por numero de fase (1, 2 o 3)
    public EnemyStats ObtenerStatsDeFase(int fase)
    {
        return fase switch
        {
            1 => statsFaseUno,
            2 => statsFaseDos,
            3 => statsFaseTres,
            _ => statsFaseUno
        };
    }
}
