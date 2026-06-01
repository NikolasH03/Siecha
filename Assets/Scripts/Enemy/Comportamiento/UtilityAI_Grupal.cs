using UnityEngine;

public enum AccionGrupal { Atacar, Flanquear, Defender, Rodear, Retirarse }

public class UtilityAI_Grupal
{
    private readonly Enemigo     enemigo;
    private readonly EnemyManager manager;
    private readonly HealthComp  salud;
    private Transform Jugador => enemigo.JugadorActual;

    public UtilityAI_Grupal(Enemigo enemigo, EnemyManager manager)
    {
        this.enemigo = enemigo;
        this.manager = manager;
        this.salud   = enemigo.GetHealthComp();
    }

    public AccionGrupal DecidirAccion()
    {
        float uAtacar   = CalcularUtilidadAtacar();
        float uRodear   = CalcularUtilidadRodear();
        float uRetirada = CalcularUtilidadRetirarse();
        
        if (uAtacar >= uRodear && uAtacar >= uRetirada) return AccionGrupal.Atacar;
        if (uRodear >= uRetirada)                        return AccionGrupal.Rodear;
        return AccionGrupal.Retirarse;
    }

    public float CalcularUtilidadAtacar()
    {
        if (Jugador == null) return 0f;

        float utilidad  = 0.5f;
        float distancia = Vector3.Distance(enemigo.transform.position, Jugador.position);

        // Más cerca = más utilidad (inversamente proporcional, normalizado a 15m)
        utilidad += (1f - Mathf.Clamp01(distancia / 15f)) * 0.5f;

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad -= 0.4f;
        if (distancia <= enemigo.RangoDeAtaque)  utilidad += 0.2f;

        return Mathf.Clamp01(utilidad);
    }

    private float CalcularUtilidadRodear()
    {
        if (Jugador == null) return 0f;

        // FIX CRÍTICO: antes la base era 1.5f → después del Clamp01 siempre
        // devolvía 1.0, haciendo que Rodear ganara casi siempre aunque hubiera
        // un atacante claro. Base correcta: 0.4f (ligeramente menor que Atacar).
        float utilidad  = 0.4f;
        float distancia = Vector3.Distance(enemigo.transform.position, Jugador.position);

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad += 0.3f;

        // Distancia ideal para rodear: 5–8m
        if (distancia >= 5f && distancia <= 8f) utilidad += 0.2f;

        return Mathf.Clamp01(utilidad);
    }

    private float CalcularUtilidadRetirarse()
    {
        float utilidad = 0.1f;

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad += 0.1f;

        // Solo retirarse de verdad si la vida es crítica (< 15%)
        if (salud != null && salud.GetVidaNormalizada() < 0.15f) utilidad += 0.5f;

        return Mathf.Clamp01(utilidad);
    }

    // Útil para debug y para el EnemyManager al seleccionar el mejor candidato
    public bool JugadorMirandoEnemigo()
    {
        if (Jugador == null) return false;
        Vector3 dir = (enemigo.transform.position - Jugador.position).normalized;
        return Vector3.Dot(Jugador.forward, dir) > 0.7f;
    }
}