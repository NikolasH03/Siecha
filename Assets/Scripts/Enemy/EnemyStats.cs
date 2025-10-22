using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Combat/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Daño de Ataques")]
    [SerializeField] private int danoAtaqueLigero = 5;
    [SerializeField] private int danoAtaqueFuerte = 15;

    [Header("Combos")]
    [SerializeField] private int maxAtaquesLigerosEnCombo = 3;
    [SerializeField] private int maxAtaquesFuertesEnCombo = 2;

    [Header("Timing")]
    [SerializeField] private float tiempoEntreAtaquesCombo = 0.5f;
    [SerializeField] private float cooldownDespuesDeCombo = 1.5f;

    // Getters públicos
    public int DanoAtaqueLigero => danoAtaqueLigero;
    public int DanoAtaqueFuerte => danoAtaqueFuerte;
    public int MaxAtaquesLigerosEnCombo => maxAtaquesLigerosEnCombo;
    public int MaxAtaquesFuertesEnCombo => maxAtaquesFuertesEnCombo;
    public float TiempoEntreAtaquesCombo => tiempoEntreAtaquesCombo;
    public float CooldownDespuesDeCombo => cooldownDespuesDeCombo;
}