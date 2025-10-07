using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HUDJugador : MonoBehaviour
{
    [SerializeField] private Image barraVida;
    [SerializeField] private Image barraEstamina;

    [SerializeField] private TextMeshProUGUI textoMuertes;

    [SerializeField] private ControladorCombate combatController;



    void Start()
    {
        if (combatController != null)
        {
            combatController.stats.OnVidaActualizada += ActualizarVida;
            combatController.stats.OnEstaminaActualizada += ActualizarEstamina;


            ActualizarVida(combatController.stats.VidaActual / combatController.stats.VidaMax);
            ActualizarEstamina(combatController.stats.EstaminaActual / combatController.stats.EstaminaMax);
        }

        ActualizarContadorMuertes();
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
        if (combatController != null)
        {
            combatController.stats.OnVidaActualizada -= ActualizarVida;
            combatController.stats.OnEstaminaActualizada -= ActualizarEstamina;
        }
    }

    public void ActualizarContadorMuertes()
    {
        if (textoMuertes != null && combatController != null)
        {
            textoMuertes.text = $"{combatController.muertesActuales}";
        }
    }

    public void Reaparecer()
    {
        combatController.Revivir();
    }
}

