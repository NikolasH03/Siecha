using UnityEngine;

/// <summary>
/// Utilidad de ataque para arqueros: la máxima está en el rango ideal de disparo
/// y baja tanto si el jugador está muy cerca (debe reposicionarse) como si está lejos.
/// </summary>
public class UtilityAI_GrupalDistancia : UtilityAI_Grupal
{
    private const float RangoIdeal   = 10f;
    private const float AnchoDeCurva = 10f;

    public UtilityAI_GrupalDistancia(EnemigoDistancia enemigo, EnemyManager manager)
        : base(enemigo, manager) { }

    public override float CalcularUtilidadAtacar()
    {
        if (Jugador == null) return 0f;

        float distancia  = Vector3.Distance(enemigo.transform.position, Jugador.position);
        float diferencia = Mathf.Abs(distancia - RangoIdeal);

        float utilidad = 0.5f + (1f - Mathf.Clamp01(diferencia / AnchoDeCurva)) * 0.5f;

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad -= 0.4f;

        return Mathf.Clamp01(utilidad);
    }
}