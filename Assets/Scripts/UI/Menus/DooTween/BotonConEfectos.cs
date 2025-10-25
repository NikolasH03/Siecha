using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class BotonConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Efectos Visuales")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Vector3 escalaClick = new Vector3(0.95f, 0.95f, 0.95f);
    [SerializeField] private float duracionAnimacion = 0.2f;
    [SerializeField] private Ease tipoEase = Ease.OutQuad;

    [Header("Colores")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorHover = Color.yellow;
    [SerializeField] private Color colorSeleccionado = Color.cyan;
    [SerializeField] private bool usarEfectoColor = true;

    [Header("Efectos Adicionales")]
    [SerializeField] private GameObject efectoSeleccion;
    [SerializeField] private bool efectoPulso = false;
    [SerializeField] private float intensidadPulso = 0.05f;

    private Vector3 escalaOriginal;
    private Color colorOriginal;
    private Image imagenBoton;
    private Button boton;
    private bool estaSeleccionado = false;
    private Tween tweenActual;
    private Tween tweenColor;
    private Tween tweenPulso;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        imagenBoton = GetComponent<Image>();
        boton = GetComponent<Button>();

        if (imagenBoton && usarEfectoColor)
        {
            colorOriginal = imagenBoton.color;
            imagenBoton.color = colorNormal;
        }

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);
    }

    private void OnDestroy()
    {
        // Limpiar tweens al destruir
        tweenActual?.Kill();
        tweenColor?.Kill();
        tweenPulso?.Kill();
    }

    // EVENTOS DE MOUSE
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaHover);
            if (usarEfectoColor) CambiarColor(colorHover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaOriginal);
            if (usarEfectoColor) CambiarColor(colorNormal);
        }
    }

    // EVENTOS DE MANDO/TECLADO
    public void OnSelect(BaseEventData eventData)
    {
        if (boton && !boton.interactable) return;

        estaSeleccionado = true;
        AnimarEscala(escalaSeleccionado);
        if (usarEfectoColor) CambiarColor(colorSeleccionado);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(true);

        if (efectoPulso)
            IniciarPulso();

        // AudioManager.Instance?.PlaySFX("select");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        AnimarEscala(escalaOriginal);
        if (usarEfectoColor) CambiarColor(colorNormal);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);

        DetenerPulso();
    }

    // EVENTOS DE CLICK
    public void OnPointerDown(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        AnimarEscala(escalaClick);
        // AudioManager.Instance?.PlaySFX("click");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        AnimarEscala(estaSeleccionado ? escalaSeleccionado : escalaHover);
    }

    private void AnimarEscala(Vector3 escalaObjetivo)
    {
        tweenActual?.Kill();
        tweenActual = transform.DOScale(escalaObjetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true); // Para que funcione con Time.timeScale = 0
    }

    private void CambiarColor(Color colorObjetivo)
    {
        if (imagenBoton)
        {
            tweenColor?.Kill();
            tweenColor = imagenBoton.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }

    private void IniciarPulso()
    {
        if (!efectoPulso) return;

        tweenPulso?.Kill();
        Vector3 escalaPulso = escalaSeleccionado + Vector3.one * intensidadPulso;

        tweenPulso = transform.DOScale(escalaPulso, duracionAnimacion * 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void DetenerPulso()
    {
        tweenPulso?.Kill();
    }
}