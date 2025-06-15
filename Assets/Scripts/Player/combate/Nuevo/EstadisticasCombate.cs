using System;
using UnityEngine;

public class EstadisticasCombate
{
    public float VidaActual { get; private set; }
    public float EstaminaActual { get; private set; }

    public float VidaMax { get; private set; }
    public float EstaminaMax { get; private set; }

    public float DanoBase { get; private set; }
    public float CuracionEstamina { get; private set; }


    public event Action<float> OnVidaActualizada;
    public event Action<float> OnEstaminaActualizada;

    public EstadisticasCombate(EstadisticasCombateSO statsBase)
    {
        VidaMax = statsBase.vidaBase;
        EstaminaMax = statsBase.estaminaBase;
        DanoBase = statsBase.danoBase;
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
    }

    public void AumentarEstaminaMax(float cantidad)
    {
        EstaminaMax += cantidad;
        EstaminaActual = Mathf.Min(EstaminaActual, EstaminaMax);
    }
}

