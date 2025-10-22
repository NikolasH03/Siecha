using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ToggleConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
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
    [SerializeField] private Color colorActivado = Color.green;
    [SerializeField] private bool usarEfectoColor = true;

    [Header("Efectos Toggle Específicos")]
    [SerializeField] private GameObject efectoSeleccion;
    [SerializeField] private GameObject efectoActivado; // Efecto cuando está ON
    [SerializeField] private bool animarCheckmark = true;
    [SerializeField] private float escalaCheckmarkActivado = 1.2f;

    private Vector3 escalaOriginal;
    private Image imagenToggle;
    private Image checkmarkImage;
    private Toggle toggle;
    private bool estaSeleccionado = false;
    private Tween tweenEscala;
    private Tween tweenColor;
    private Tween tweenCheckmark;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        imagenToggle = GetComponent<Image>();
        toggle = GetComponent<Toggle>();

        // Buscar el checkmark (usualmente está en un child)
        checkmarkImage = GetComponentInChildren<Image>();
        if (checkmarkImage == imagenToggle) // Si es la misma imagen, buscar otra
        {
            var images = GetComponentsInChildren<Image>();
            checkmarkImage = images.Length > 1 ? images[1] : null;
        }

        if (imagenToggle && usarEfectoColor)
        {
            imagenToggle.color = colorNormal;
        }

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);

        if (efectoActivado)
            efectoActivado.SetActive(toggle?.isOn ?? false);

        // Suscribirse al evento de cambio del toggle
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColor?.Kill();
        tweenCheckmark?.Kill();

        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    private void OnToggleChanged(bool valor)
    {
        // Animar el checkmark
        if (animarCheckmark && checkmarkImage != null)
        {
            tweenCheckmark?.Kill();
            Vector3 escalaObjetivo = valor ? Vector3.one * escalaCheckmarkActivado : Vector3.one;
            tweenCheckmark = checkmarkImage.transform.DOScale(escalaObjetivo, duracionAnimacion)
                .SetEase(Ease.OutBounce)
                .SetUpdate(true);
        }

        // Cambiar color si está activado
        if (usarEfectoColor && valor)
        {
            CambiarColor(colorActivado);
        }

        // Mostrar/ocultar efecto de activado
        if (efectoActivado)
        {
            efectoActivado.SetActive(valor);
        }
    }

    // EVENTOS DE MOUSE
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toggle && !toggle.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaHover);
            if (usarEfectoColor && !toggle.isOn) CambiarColor(colorHover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (toggle && !toggle.interactable) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaOriginal);
            if (usarEfectoColor && !toggle.isOn) CambiarColor(colorNormal);
        }
    }

    // EVENTOS DE MANDO/TECLADO
    public void OnSelect(BaseEventData eventData)
    {
        if (toggle && !toggle.interactable) return;

        estaSeleccionado = true;
        AnimarEscala(escalaSeleccionado);
        if (usarEfectoColor && !toggle.isOn) CambiarColor(colorSeleccionado);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        AnimarEscala(escalaOriginal);

        Color colorFinal = toggle.isOn ? colorActivado : colorNormal;
        if (usarEfectoColor) CambiarColor(colorFinal);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);
    }

    // EVENTOS DE CLICK
    public void OnPointerDown(PointerEventData eventData)
    {
        if (toggle && !toggle.interactable) return;
        AnimarEscala(escalaClick);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (toggle && !toggle.interactable) return;
        AnimarEscala(estaSeleccionado ? escalaSeleccionado : escalaHover);
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
        if (imagenToggle)
        {
            tweenColor?.Kill();
            tweenColor = imagenToggle.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }
}
