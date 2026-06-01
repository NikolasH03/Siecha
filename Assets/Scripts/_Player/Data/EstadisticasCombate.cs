using System;
using UnityEngine;

public class EstadisticasCombate
{
    public float VidaActual;
    public float EstaminaActual;

    public float VidaMax;
    public float EstaminaMax;
    public float CuracionEstamina { get; private set; }


    public event Action<float> OnVidaActualizada;
    public event Action<float> OnEstaminaActualizada;

    public EstadisticasCombate(EstadisticasCombateSO statsBase)
    {

        VidaMax = statsBase.vidaBase;
        EstaminaMax = statsBase.estaminaBase;
        CuracionEstamina = statsBase.curacionEstamina;

        VidaActual = VidaMax;
        EstaminaActual = EstaminaMax;
    }

    public void RecibirDano(float cantidad)
    {
        VidaActual = Mathf.Max(VidaActual - cantidad, 0);
        OnVidaActualizada?.Invoke(VidaActual / VidaMax);
    }

    public void CurarVida(float cantidad)
    {
        VidaActual = Mathf.Min(VidaMax, VidaActual + cantidad);
        OnVidaActualizada?.Invoke(VidaActual / VidaMax);
    }
    public void CurarPorcentajeVida()
    {
        VidaActual = Mathf.Min(VidaMax, VidaActual + VidaMax*0.15f);
        OnVidaActualizada?.Invoke(VidaActual / VidaMax);
    }

    public void UsarEstamina(float cantidad)
    {
        EstaminaActual = Mathf.Max(EstaminaActual - cantidad, 0);
        OnEstaminaActualizada?.Invoke(EstaminaActual / EstaminaMax);
    }

    public void RegenerarEstamina(float cantidad)
    {
        EstaminaActual = Mathf.Min(EstaminaActual + cantidad, EstaminaMax);
        OnEstaminaActualizada?.Invoke(EstaminaActual / EstaminaMax);
    }

    public void AumentarVidaMax(float cantidad)
    {
        VidaMax += cantidad;
        VidaActual = Mathf.Min(VidaActual, VidaMax);
        OnVidaActualizada?.Invoke(VidaActual / VidaMax);
    }

    public void AumentarEstaminaMax(float cantidad)
    {
        EstaminaMax += cantidad;
        EstaminaActual = Mathf.Min(EstaminaActual, EstaminaMax);
        OnEstaminaActualizada?.Invoke(EstaminaActual / EstaminaMax);
    }
    public void CargarEstadisticasActuales(float vidaMax, float estaminaMax, float vidaActual)
    {
        VidaMax = vidaMax;
        EstaminaMax = estaminaMax;
        VidaActual = vidaActual;
    }
}

