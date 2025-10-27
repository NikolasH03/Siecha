using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class BotonConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI textoBoton;
    [SerializeField] private Image efectoHalo;

    [Header("Efectos Visuales - Escala")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.03f, 1.03f, 1.03f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.02f, 1.02f, 1.02f);
    [SerializeField] private Vector3 escalaClick = new Vector3(0.98f, 0.98f, 0.98f);
    [SerializeField] private float duracionAnimacion = 0.25f;
    [SerializeField] private Ease tipoEase = Ease.OutCubic;

    [Header("Colores del Texto")]
    [SerializeField] private Color colorNormal = new Color(0.91f, 0.89f, 0.83f, 0.7f);
    [SerializeField] private Color colorHover = new Color(0.91f, 0.89f, 0.83f, 1f);
    [SerializeField] private Color colorSeleccionado = new Color(0.91f, 0.89f, 0.83f, 1f);

    [Header("Efecto Halo")]
    [SerializeField] private bool usarEfectoHalo = true;
    [SerializeField] private float duracionHalo = 0.4f;
    [SerializeField] private float alphaHaloMax = 3f;
    [SerializeField] private float escalaHaloHover = 1.2f;
    [SerializeField] private Ease easeHalo = Ease.OutQuad;

    [Header("Efectos Adicionales")]
    [SerializeField] private bool efectoPulso = true;
    [SerializeField] private float intensidadPulso = 0.015f;

    private Vector3 escalaOriginal;
    private Vector3 escalaOriginalHalo;
    private Button boton;
    private bool estaEnHover = false;
    private bool estaSeleccionado = false;

    private Tween tweenEscala;
    private Tween tweenColor;
    private Tween tweenPulso;
    private Sequence secuenciaHalo;

    private void Awake()
    {
        boton = GetComponent<Button>();

        if (textoBoton == null)
            textoBoton = GetComponentInChildren<TextMeshProUGUI>();

        if (efectoHalo != null)
        {
            efectoHalo.preserveAspect = true;
            escalaOriginalHalo = efectoHalo.rectTransform.localScale;
            Color c = efectoHalo.color;
            c.a = 0;
            efectoHalo.color = c;
            efectoHalo.gameObject.SetActive(false);
        }

        efectoHalo.rectTransform.localScale = new Vector3(
        escalaOriginalHalo.x * 0.85f,  // ancho un 15% más estrecho
        escalaOriginalHalo.y,          // altura igual
        escalaOriginalHalo.z
        );
        escalaOriginalHalo = efectoHalo.rectTransform.localScale; // actualizar la referencia
        // Fondo transparente (por si hay una imagen detrás)
        Image fondo = GetComponent<Image>();
        if (fondo != null)
            fondo.color = new Color(1, 1, 1, 0);

        if (efectoHalo != null)
        {
            efectoHalo.raycastTarget = false;
        }

    }

    private void Start()
    {
        escalaOriginal = transform.localScale;
        if (textoBoton != null)
            textoBoton.color = colorNormal;
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColor?.Kill();
        tweenPulso?.Kill();
        secuenciaHalo?.Kill();
    }

    // ==================== EVENTOS ====================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        estaEnHover = true;
        if (!estaSeleccionado)
            AnimarHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        estaEnHover = false;
        if (!estaSeleccionado)
            AnimarHover(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (boton && !boton.interactable) return;
        estaSeleccionado = true;
        AnimarSeleccion(true);
        if (efectoPulso) IniciarPulso();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        DetenerPulso();
        
        // Si aún está en hover, mantener estado hover, sino volver a normal
        if (estaEnHover)
            AnimarHover(true);
        else
            AnimarSeleccion(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        AnimarEscala(escalaClick);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        AnimarEscala(estaSeleccionado ? escalaSeleccionado : escalaHover);
    }

    // ==================== ANIMACIONES ====================
    private void AnimarHover(bool entrar)
    {
        if (entrar)
        {
            AnimarEscala(escalaHover);
            CambiarColorTexto(colorHover);
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHalo(true);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            CambiarColorTexto(colorNormal);
            if (efectoHalo != null)
                MostrarHalo(false);
        }
    }

    private void AnimarSeleccion(bool seleccionar)
    {
        if (seleccionar)
        {
            AnimarEscala(escalaSeleccionado);
            CambiarColorTexto(colorSeleccionado);
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHalo(true);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            CambiarColorTexto(colorNormal);
            if (efectoHalo != null)
                MostrarHalo(false);
        }
    }

    private void AnimarEscala(Vector3 objetivo)
    {
        tweenEscala?.Kill();
        tweenEscala = transform.DOScale(objetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }

    private void CambiarColorTexto(Color color)
    {
        if (textoBoton == null) return;
        
        tweenColor?.Kill();
        tweenColor = textoBoton.DOColor(color, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }

    // ==================== HALO ====================
    private void MostrarHalo(bool mostrar)
    {
        if (efectoHalo == null)
        {
            efectoHalo = transform.GetComponentInChildren<Image>(true);
            if (efectoHalo == null) return;
        }

        secuenciaHalo?.Kill();

        // Siempre mantener activo mientras haya hover o selección
        if (mostrar)
        {
            efectoHalo.gameObject.SetActive(true);

            Color c = efectoHalo.color;
            c.a = 0;
            efectoHalo.color = c;

            efectoHalo.rectTransform.localScale = escalaOriginalHalo * 1f;

            float escalaFinal = escalaOriginalHalo.x * escalaHaloHover;

            secuenciaHalo = DOTween.Sequence()
                .Join(efectoHalo.DOFade(alphaHaloMax, duracionHalo))
                .Join(efectoHalo.rectTransform.DOScale(Vector3.one * escalaFinal, duracionHalo))
                .SetEase(easeHalo)
                .SetUpdate(true);
        }
        else
        {
            // Solo ocultar si realmente no hay hover ni selección
            if (!estaSeleccionado && !estaEnHover)
            {
                secuenciaHalo = DOTween.Sequence()
                    .Join(efectoHalo.DOFade(0f, duracionHalo * 0.6f))
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        efectoHalo.gameObject.SetActive(false);
                    });
            }
        }
    }

    // ==================== PULSO ====================
    private void IniciarPulso()
    {
        if (!efectoPulso) return;

        tweenPulso?.Kill();

        Vector3 escalaPulso = escalaSeleccionado + Vector3.one * intensidadPulso;
        tweenPulso = transform.DOScale(escalaPulso, duracionAnimacion * 3f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);

        // Asegura que el halo permanezca visible durante el pulso
        if (usarEfectoHalo && efectoHalo != null && !efectoHalo.gameObject.activeSelf)
            MostrarHalo(true);
    }


    private void DetenerPulso()
    {
        tweenPulso?.Kill();
        // Volver suavemente a la escala correspondiente
        if (estaSeleccionado)
            transform.DOScale(escalaSeleccionado, duracionAnimacion * 0.5f).SetUpdate(true);
    }
}