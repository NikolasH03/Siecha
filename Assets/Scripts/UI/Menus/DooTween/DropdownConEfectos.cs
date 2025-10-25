using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DropdownConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Efectos Visuales")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.03f, 1.03f, 1.03f);
    [SerializeField] private Vector3 escalaClick = new Vector3(0.97f, 0.97f, 0.97f);
    [SerializeField] private float duracionAnimacion = 0.2f;
    [SerializeField] private Ease tipoEase = Ease.OutQuad;

    [Header("Colores")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorHover = Color.yellow;
    [SerializeField] private Color colorSeleccionado = Color.cyan;
    [SerializeField] private bool usarEfectoColor = true;

    [Header("Efectos Flecha")]
    [SerializeField] private bool animarFlecha = true;
    [SerializeField] private Vector3 rotacionFlechaAbierta = new Vector3(0, 0, 180);

    private Vector3 escalaOriginal;
    private Image imagenDropdown;
    private Image imagenFlecha;
    private Dropdown dropdown;
    private bool estaSeleccionado = false;
    private bool estaAbierto = false;
    private Tween tweenEscala;
    private Tween tweenColor;
    private Tween tweenFlecha;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        imagenDropdown = GetComponent<Image>();
        dropdown = GetComponent<Dropdown>();

        // Buscar la flecha (usualmente es la segunda imagen)
        var images = GetComponentsInChildren<Image>();
        imagenFlecha = images.Length > 1 ? images[1] : null;

        if (imagenDropdown && usarEfectoColor)
        {
            imagenDropdown.color = colorNormal;
        }

        // Suscribirse al evento de dropdown
        if (dropdown != null)
        {
            // En versiones más nuevas de Unity, usar dropdown.onValueChanged
            // Para detectar cuando se abre/cierra, necesitarías eventos adicionales
        }
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColor?.Kill();
        tweenFlecha?.Kill();
    }

    // EVENTOS DE MOUSE
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dropdown && !dropdown.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaHover);
            if (usarEfectoColor) CambiarColor(colorHover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (dropdown && !dropdown.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaOriginal);
            if (usarEfectoColor) CambiarColor(colorNormal);
        }
    }

    // EVENTOS DE MANDO/TECLADO
    public void OnSelect(BaseEventData eventData)
    {
        if (dropdown && !dropdown.interactable) return;

        estaSeleccionado = true;
        AnimarEscala(escalaSeleccionado);
        if (usarEfectoColor) CambiarColor(colorSeleccionado);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        AnimarEscala(escalaOriginal);
        if (usarEfectoColor) CambiarColor(colorNormal);
    }

    // EVENTOS DE CLICK
    public void OnPointerDown(PointerEventData eventData)
    {
        if (dropdown && !dropdown.interactable) return;
        AnimarEscala(escalaClick);
        AnimarFlecha(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (dropdown && !dropdown.interactable) return;
        AnimarEscala(estaSeleccionado ? escalaSeleccionado : escalaHover);
        AnimarFlecha(false);
    }

    private void AnimarEscala(Vector3 escalaObjetivo)
    {
        tweenEscala?.Kill();
        tweenEscala = transform.DOScale(escalaObjetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }

    private void CambiarColor(Color colorObjetivo)
    {
        if (imagenDropdown)
        {
            tweenColor?.Kill();
            tweenColor = imagenDropdown.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }

    private void AnimarFlecha(bool abierto)
    {
        if (!animarFlecha || imagenFlecha == null) return;

        tweenFlecha?.Kill();
        Vector3 rotacionObjetivo = abierto ? rotacionFlechaAbierta : Vector3.zero;
        tweenFlecha = imagenFlecha.transform.DORotate(rotacionObjetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }
}
