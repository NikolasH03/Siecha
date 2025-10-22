using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SliderConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Efectos Visuales")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.02f, 1.1f, 1f); // Más alto que ancho
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.01f, 1.05f, 1f);
    [SerializeField] private float duracionAnimacion = 0.2f;
    [SerializeField] private Ease tipoEase = Ease.OutQuad;

    [Header("Colores Handle")]
    [SerializeField] private Color colorHandleNormal = Color.white;
    [SerializeField] private Color colorHandleHover = Color.yellow;
    [SerializeField] private Color colorHandleSeleccionado = Color.cyan;
    [SerializeField] private bool usarEfectoColorHandle = true;

    [Header("Colores Fill")]
    [SerializeField] private Color colorFillNormal = Color.blue;
    [SerializeField] private Color colorFillHover = Color.cyan;
    [SerializeField] private Color colorFillSeleccionado = Color.green;
    [SerializeField] private bool usarEfectoColorFill = true;

    [Header("Efectos Adicionales")]
    [SerializeField] private GameObject efectoSeleccion;
    [SerializeField] private bool efectoPulsoHandle = false;
    [SerializeField] private Vector3 escalaPulsoHandle = new Vector3(1.2f, 1.2f, 1f);

    private Vector3 escalaOriginal;
    private Image imagenHandle;
    private Image imagenFill;
    private Slider slider;
    private bool estaSeleccionado = false;
    private Tween tweenEscala;
    private Tween tweenColorHandle;
    private Tween tweenColorFill;
    private Tween tweenPulsoHandle;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        slider = GetComponent<Slider>();

        // Buscar componentes del slider
        if (slider != null)
        {
            imagenHandle = slider.handleRect?.GetComponent<Image>();
            imagenFill = slider.fillRect?.GetComponent<Image>();
        }

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColorHandle?.Kill();
        tweenColorFill?.Kill();
        tweenPulsoHandle?.Kill();
    }

    // EVENTOS DE MOUSE
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slider && !slider.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaHover);
            if (usarEfectoColorHandle) CambiarColorHandle(colorHandleHover);
            if (usarEfectoColorFill) CambiarColorFill(colorFillHover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slider && !slider.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaOriginal);
            if (usarEfectoColorHandle) CambiarColorHandle(colorHandleNormal);
            if (usarEfectoColorFill) CambiarColorFill(colorFillNormal);
        }
    }

    // EVENTOS DE MANDO/TECLADO
    public void OnSelect(BaseEventData eventData)
    {
        if (slider && !slider.interactable) return;

        estaSeleccionado = true;
        AnimarEscala(escalaSeleccionado);
        if (usarEfectoColorHandle) CambiarColorHandle(colorHandleSeleccionado);
        if (usarEfectoColorFill) CambiarColorFill(colorFillSeleccionado);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(true);

        if (efectoPulsoHandle)
            IniciarPulsoHandle();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        AnimarEscala(escalaOriginal);
        if (usarEfectoColorHandle) CambiarColorHandle(colorHandleNormal);
        if (usarEfectoColorFill) CambiarColorFill(colorFillNormal);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);

        DetenerPulsoHandle();
    }

    private void AnimarEscala(Vector3 escalaObjetivo)
    {
        tweenEscala?.Kill();
        tweenEscala = transform.DOScale(escalaObjetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }

    private void CambiarColorHandle(Color colorObjetivo)
    {
        if (imagenHandle)
        {
            tweenColorHandle?.Kill();
            tweenColorHandle = imagenHandle.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }

    private void CambiarColorFill(Color colorObjetivo)
    {
        if (imagenFill)
        {
            tweenColorFill?.Kill();
            tweenColorFill = imagenFill.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }

    private void IniciarPulsoHandle()
    {
        if (!efectoPulsoHandle || imagenHandle == null) return;

        tweenPulsoHandle?.Kill();
        tweenPulsoHandle = imagenHandle.transform.DOScale(escalaPulsoHandle, duracionAnimacion * 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void DetenerPulsoHandle()
    {
        tweenPulsoHandle?.Kill();
        if (imagenHandle != null)
        {
            imagenHandle.transform.localScale = Vector3.one;
        }
    }
}