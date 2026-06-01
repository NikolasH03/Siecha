using UnityEngine;

public enum TipoDecisionTactica { AtaqueLigero, AtaqueFuerte, Esquivar, Bloquear, Ninguna }

public class UtilityAI_Tactico
{
    private readonly Enemigo    enemigo;
    private readonly HealthComp salud;

    private Transform Jugador => enemigo.JugadorActual;
    private DetectorObjetivoJugador Detector => EnemyManager.instance?.DetectorObjetivo;

    // Acceso a las capacidades del enemigo según sus stats actuales.
    // Para el jefe: cada fase tiene diferentes capacidades configuradas en el SO.
    private bool PuedeBloquear => enemigo.Stats != null && enemigo.Stats.PuedeBloquear;
    private bool PuedeEsquivar => enemigo.Stats != null && enemigo.Stats.PuedeEsquivar;

    public UtilityAI_Tactico(Enemigo enemigo)
    {
        this.enemigo = enemigo;
        this.salud   = enemigo.GetHealthComp();
    }

    public TipoDecisionTactica DecidirAccionTactica()
    {
        float uLigero   = UtilidadAtaqueLigero();
        float uFuerte   = UtilidadAtaqueFuerte();
        float uEsquivar = UtilidadEsquivar();
        float uBloquear = UtilidadBloquear();

        float max = Mathf.Max(uLigero, uFuerte, uEsquivar, uBloquear);

        if (max <= 0.1f) return TipoDecisionTactica.Ninguna;

        if (SoyElObjetivoActual())
        {
            if (uEsquivar >= uBloquear && uEsquivar == max) return TipoDecisionTactica.Esquivar;
            if (uBloquear == max)                           return TipoDecisionTactica.Bloquear;
        }

        if (uFuerte >= uLigero && uFuerte == max) return TipoDecisionTactica.AtaqueFuerte;
        return TipoDecisionTactica.AtaqueLigero;
    }

    public float UtilidadAtaqueLigero()
    {
        float u = 0.6f;
        if (!JugadorBloqueando()) u += 0.2f;
        if (salud.GetVidaNormalizada() > 0.5f) u += 0.2f;
        if (JugadorBloqueando()) u -= 0.5f;
        return Mathf.Clamp01(u);
    }

    public float UtilidadAtaqueFuerte()
    {
        float u = 0.4f;
        if (JugadorBloqueando())               u += 0.6f;
        if (salud.GetVidaNormalizada() > 0.5f) u += 0.2f;
        if (salud.GetVidaNormalizada() < 0.3f) u -= 0.5f;
        return Mathf.Clamp01(u);
    }

    public float UtilidadEsquivar()
    {
        // Respetar capacidad del SO — Fase 2 del jefe (escudo) no esquiva
        if (!PuedeEsquivar)                                return 0f;
        if (!JugadorAtacando() || !SoyElObjetivoActual()) return 0f;

        float u    = 0.6f;
        float vida = salud.GetVidaNormalizada();
        if (vida > 0.5f && vida < 0.8f) u += 0.3f;
        if (vida <= 0.5f)               u -= 0.2f;
        return Mathf.Clamp01(u);
    }

    public float UtilidadBloquear()
    {
        // Respetar capacidad del SO — Fase 1 del jefe (cuchillos) no bloquea
        if (!PuedeBloquear)                                return 0f;
        if (salud.EnGuardBreak)                            return 0f;
        if (!JugadorAtacando() || !SoyElObjetivoActual())  return 0f;

        float u    = 0.4f;
        float vida = salud.GetVidaNormalizada();
        if (vida <= 0.5f)         u += 0.3f;
        if (salud.DebeBloquear()) u += 0.3f;
        return Mathf.Clamp01(u);
    }

    private bool SoyElObjetivoActual()
    {
        if (Detector == null) return false;
        return Detector.EsSoyElObjetivo(enemigo.transform);
    }

    private bool JugadorBloqueando()
    {
        if (Jugador == null) return false;
        var c = Jugador.GetComponent<ControladorCombate>();
        return c != null && c.getBloqueando();
    }

    private bool JugadorAtacando()
    {
        if (Jugador == null) return false;
        var c = Jugador.GetComponent<ControladorCombate>();
        return c != null && c.getAtacando();
    }
}