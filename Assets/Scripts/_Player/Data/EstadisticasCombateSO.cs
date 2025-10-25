using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Combat Stats Base")]
public class EstadisticasCombateSO : ScriptableObject
{
    [Header("Stats Base")]
    public float vidaBase = 100;
    public float estaminaBase = 100;
    public float curacionEstamina = 5;

    [Header("Cadena de ataques")]
    public int maxAtaquesLigeros = 3;
    public int maxAtaquesFuertes = 3;
}

