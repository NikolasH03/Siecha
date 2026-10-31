using UnityEngine;

public enum TipoDecisionTactica { AtaqueLigero, AtaqueFuerte, Esquivar, Bloquear, Ninguna }

/// <summary>
/// Decisiones de combate individuales (defender u ofender) mientras el jugador actúa.
/// Cada opción recibe una utilidad entre 0 y 1.
/// </summary>
public class UtilityAI_Tactico
{
    private const float UmbralDefensa = 0.4f;

    private readonly Enemigo    enemigo;
    private readonly HealthComp salud;

    private Transform Jugador => enemigo.JugadorActual;

    private static DetectorObjetivoJugador Detector =>
        EnemyManager.instance != null ? EnemyManager.instance.DetectorObjetivo : null;

    private static ControladorCombate CombateJugador =>
        EnemyManager.instance != null ? EnemyManager.instance.CombateJugador : null;

    // Las capacidades dependen de los stats activos (el jefe cambia de stats por fase).
    private bool PuedeBloquear => enemigo.Stats != null && enemigo.Stats.PuedeBloquear;
    private bool PuedeEsquivar => enemigo.Stats != null && enemigo.Stats.PuedeEsquivar;

    public UtilityAI_Tactico(Enemigo enemigo)
    {
        this.enemigo = enemigo;
        salud        = enemigo.GetHealthComp();
    }

    public TipoDecisionTactica DecidirAccionTactica()
    {
        // 1. Defensa: tiene prioridad sobre cualquier ataque.
        // TODO(IA): hoy nunca devuelve Esquivar; cualquier defensa se resuelve como Bloquear.
        if (UtilidadEsquivar() > UmbralDefensa || UtilidadBloquear() > UmbralDefensa)
            return TipoDecisionTactica.Bloquear;

        // 2. Ofensiva.
        float uLigero = UtilidadAtaqueLigero();
        float uFuerte = UtilidadAtaqueFuerte();

        if (Mathf.Max(uLigero, uFuerte) <= 0.1f) return TipoDecisionTactica.Ninguna;

        return uLigero >= uFuerte ? TipoDecisionTactica.AtaqueLigero : TipoDecisionTactica.AtaqueFuerte;
    }

    // ─── Utilidades ofensivas ─────────────────────────────────────────────────

    public float UtilidadAtaqueLigero()
    {
        float u = 0.6f;

        if (JugadorBloqueando()) u -= 0.5f;
        else                     u += 0.2f;

        if (salud.GetVidaNormalizada() > 0.5f) u += 0.2f;

        return Mathf.Clamp01(u);
    }

    public float UtilidadAtaqueFuerte()
    {
        float u    = 0.4f;
        float vida = salud.GetVidaNormalizada();

        if (JugadorBloqueando()) u += 0.6f;
        if (vida > 0.5f)         u += 0.2f;
        if (vida < 0.3f)         u -= 0.5f;

        return Mathf.Clamp01(u);
    }

    // ─── Utilidades defensivas ────────────────────────────────────────────────

    public float UtilidadBloquear()
    {
        if (!PuedeBloquear || salud.EnGuardBreak) return 0f;

        if (JugadorAtacando() && SoyElObjetivoActual())
        {
            float u = 0.5f;
            if (salud.GetVidaNormalizada() <= 0.6f) u += 0.3f;
            return Mathf.Clamp01(u);
        }

        return JugadorCercaYMirando() ? 0.9f : 0f;
    }

    public float UtilidadEsquivar()
    {
        if (!PuedeEsquivar) return 0f;

        float u = 0.2f;

        // Los arqueros esquivan más si el jugador se les acerca.
        if (enemigo is EnemigoDistancia && JugadorCercaYMirando())
        {
            u += 0.6f;
        }
        else
        {
            float vida = salud.GetVidaNormalizada();

            if (vida > 0.5f && vida < 0.8f) u += 0.3f;
            if (vida <= 0.6f)               u -= 0.2f; // con poca vida prefieren bloquear
            if (JugadorCercaYMirando())     u += 0.3f;
        }

        return Mathf.Clamp01(u);
    }

    // ─── Consultas sobre el jugador ───────────────────────────────────────────

    /// <summary>Jugador a menos de 5 m y mirando al enemigo (cono frontal de ~45°).</summary>
    private bool JugadorCercaYMirando()
    {
        if (Jugador == null) return false;

        Vector3 haciaEnemigo = enemigo.transform.position - Jugador.position;
        float   distancia    = haciaEnemigo.magnitude;

        return distancia < 5f && Vector3.Dot(Jugador.forward, haciaEnemigo.normalized) > 0.7f;
    }

    private bool SoyElObjetivoActual()
    {
        var detector = Detector;
        return detector != null && detector.EsSoyElObjetivo(enemigo.transform);
    }

    private bool JugadorBloqueando()
    {
        var combate = CombateJugador;
        return combate != null && combate.getBloqueando();
    }

    private bool JugadorAtacando()
    {
        var combate = CombateJugador;
        return combate != null && combate.getAtacando();
    }
}