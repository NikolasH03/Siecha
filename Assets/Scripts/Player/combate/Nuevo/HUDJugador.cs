using UnityEngine;
using UnityEngine.UI;
public class HUDJugador : MonoBehaviour
{
    [SerializeField] private Image barraVida;
    [SerializeField] private Image barraEstamina;
    [SerializeField] private ControladorCombate controlador;

    void Start()
    {
        if (controlador != null)
        {
            controlador.stats.OnVidaActualizada += ActualizarVida;
            controlador.stats.OnEstaminaActualizada += ActualizarEstamina;


            ActualizarVida(controlador.stats.VidaActual / controlador.stats.VidaMax);
            ActualizarEstamina(controlador.stats.EstaminaActual / controlador.stats.EstaminaMax);
        }
    }

    void ActualizarVida(float porcentaje)
    {
        barraVida.fillAmount = porcentaje;
    }

    void ActualizarEstamina(float porcentaje)
    {
        barraEstamina.fillAmount = porcentaje;
    }

    void OnDestroy()
    {
        if (controlador != null)
        {
            controlador.stats.OnVidaActualizada -= ActualizarVida;
            controlador.stats.OnEstaminaActualizada -= ActualizarEstamina;
        }
    }
}

