using UnityEngine;

public enum TipoDecisionTactica { AtaqueLigero, AtaqueFuerte, Esquivar, Bloquear, Ninguna }

public class UtilityAI_Tactico
{
    private Enemigo enemigo;
    private HealthComp salud;
    private Transform jugador;

    public UtilityAI_Tactico(Enemigo enemigo)
    {
        this.enemigo = enemigo;
        this.salud = enemigo.GetComponent<HealthComp>();
        this.jugador = enemigo.JugadorActual;
    }

    public TipoDecisionTactica DecidirAccionTactica()
    {
        float uLigero = UtilidadAtaqueLigero();
        float uFuerte = UtilidadAtaqueFuerte();
        float uEsquivar = UtilidadEsquivar();
        float uBloquear = UtilidadBloquear();

        float max = Mathf.Max(uLigero, uFuerte, uEsquivar, uBloquear);

        if (max <= 0.1f) return TipoDecisionTactica.Ninguna;

        if (JugadorAtacando())
        {
            if (max == uEsquivar) return TipoDecisionTactica.Esquivar;
            if (max == uBloquear) return TipoDecisionTactica.Bloquear;
        }

        if (max == uFuerte) return TipoDecisionTactica.AtaqueFuerte;

        return TipoDecisionTactica.AtaqueLigero;
    }

    public float UtilidadAtaqueLigero()
    {
        float utilidad = 0.6f;

        if (!JugadorBloqueando()) utilidad += 0.2f;

        float vidaRatio = salud.GetVidaNormalizada();
        if (vidaRatio > 0.5f) utilidad += 0.2f;

        if (JugadorBloqueando()) utilidad -= 0.5f;

        return Mathf.Clamp01(utilidad);
    }

    public float UtilidadAtaqueFuerte()
    {
        float utilidad = 0.4f;

        if (JugadorBloqueando()) utilidad += 0.6f;

        float vidaRatio = salud.GetVidaNormalizada();
        if (vidaRatio > 0.5f) utilidad += 0.2f;
        if (vidaRatio < 0.3f) utilidad -= 0.5f;

        return Mathf.Clamp01(utilidad);
    }

    public float UtilidadEsquivar()
    {
        float utilidad = 0f;

        if (!JugadorAtacando()) return 0f;

        utilidad += 0.6f;

        float vidaRatio = salud.GetVidaNormalizada();

        if (vidaRatio < 0.8f && vidaRatio > 0.5f)
        {
            utilidad += 0.3f; // Vida media-alta = mas esquivar
        }

        if (vidaRatio <= 0.5f)
        {
            utilidad -= 0.2f; // Vida critica = mejor bloquear
        }

        return Mathf.Clamp01(utilidad);
    }

    public float UtilidadBloquear()
    {
        float utilidad = 0f;

        if (!JugadorAtacando()) return 0f;

        utilidad += 0.4f;

        float vidaRatio = salud.GetVidaNormalizada();

        if (vidaRatio <= 0.5f)
            utilidad += 0.3f; // Vida menor a la mitad = bloquear

        if (salud.DebeBloquear())
            utilidad += 0.3f;

        if (salud.EnGuardBreak)
            return 0f;

        return Mathf.Clamp01(utilidad);
    }
    private bool JugadorBloqueando()
    {
        var combate = jugador.GetComponent<ControladorCombate>();
        return combate != null && combate.getBloqueando();
    }

    private bool JugadorAtacando()
    {
        var combate = jugador.GetComponent<ControladorCombate>();
        return combate != null && combate.getAtacando();
    }
}
