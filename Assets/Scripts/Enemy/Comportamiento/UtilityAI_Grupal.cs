using UnityEngine;

public enum AccionGrupal { Atacar, Flanquear, Defender, Rodear, Retirarse }

/// <summary>
/// Decide qué hace el enemigo respecto al grupo: atacar, rodear o retirarse.
/// Cada acción recibe una utilidad entre 0 y 1; gana la mayor.
/// </summary>
public class UtilityAI_Grupal
{
    private const float DistanciaMaximaUtilidad = 15f;
    private const float VidaCritica             = 0.15f;

    protected readonly Enemigo enemigo;
    private readonly HealthComp salud;

    protected Transform Jugador => enemigo.JugadorActual;

    // El parámetro manager se conserva para no romper otros constructores existentes (p. ej. jefe).
    public UtilityAI_Grupal(Enemigo enemigo, EnemyManager manager)
    {
        this.enemigo = enemigo;
        salud        = enemigo.GetHealthComp();
    }

    public AccionGrupal DecidirAccion()
    {
        float uAtacar   = CalcularUtilidadAtacar();
        float uRodear   = CalcularUtilidadRodear();
        float uRetirada = CalcularUtilidadRetirarse();

        if (uAtacar >= uRodear && uAtacar >= uRetirada) return AccionGrupal.Atacar;
        if (uRodear >= uRetirada)                       return AccionGrupal.Rodear;
        return AccionGrupal.Retirarse;
    }

    // ─── Utilidades ───────────────────────────────────────────────────────────

    /// <summary>También la usa EnemyManager para elegir quién recibe un slot de ataque.</summary>
    public virtual float CalcularUtilidadAtacar()
    {
        if (Jugador == null) return 0f;

        float distancia = Vector3.Distance(enemigo.transform.position, Jugador.position);

        // Más cerca = más utilidad.
        float utilidad = 0.5f + (1f - Mathf.Clamp01(distancia / DistanciaMaximaUtilidad)) * 0.5f;

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad -= 0.4f;
        if (distancia <= enemigo.RangoDeAtaque)  utilidad += 0.2f;

        return Mathf.Clamp01(utilidad);
    }

    private float CalcularUtilidadRodear()
    {
        if (Jugador == null) return 0f;

        // Base menor que la de atacar: rodear solo gana cuando atacar no conviene.
        float utilidad  = 0.4f;
        float distancia = Vector3.Distance(enemigo.transform.position, Jugador.position);

        if (!enemigo.EstaDisponibleParaAtacar()) utilidad += 0.3f;
        if (distancia >= 5f && distancia <= 8f)  utilidad += 0.2f; // distancia ideal para rodear

        return Mathf.Clamp01(utilidad);
    }

    private float CalcularUtilidadRetirarse()
    {
        float utilidad = 0.1f;

        if (!enemigo.EstaDisponibleParaAtacar())                         utilidad += 0.1f;
        if (salud != null && salud.GetVidaNormalizada() < VidaCritica)   utilidad += 0.5f;

        return Mathf.Clamp01(utilidad);
    }

    // ─── Consultas ────────────────────────────────────────────────────────────

    public bool JugadorMirandoEnemigo()
    {
        if (Jugador == null) return false;

        Vector3 dir = (enemigo.transform.position - Jugador.position).normalized;
        return Vector3.Dot(Jugador.forward, dir) > 0.7f;
    }
}