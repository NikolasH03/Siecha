using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodoUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image icono;
    public HabilidadData habilidad;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
        ActualizarVisual();
    }

    public void Configurar(HabilidadData data)
    {
        habilidad = data;
        icono.sprite = data.icono;
        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        bool desbloqueada = HabilidadesJugador.instance.EstaDesbloqueada(habilidad);

        icono.color = desbloqueada ? Color.white : Color.gray;
        boton.interactable = PuedeDesbloquear();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (habilidad != null)
        {
            VisualizadorNodoUI.instance.Mostrar(habilidad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        VisualizadorNodoUI.instance.Ocultar();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (habilidad == null || !PuedeDesbloquear()) return;

        bool resultado = HabilidadesJugador.instance.IntentarDesbloquear(habilidad);
        if (resultado)
        {
            Debug.Log("Habilidad desbloqueada: " + habilidad.nombre);
            ActualizarVisual();
        }
        else
        {
            Debug.Log("No se pudo desbloquear: " + habilidad.nombre);
        }
    }

    private bool PuedeDesbloquear()
    {
        return habilidad.requisito == null || HabilidadesJugador.instance.EstaDesbloqueada(habilidad.requisito);
    }
}


