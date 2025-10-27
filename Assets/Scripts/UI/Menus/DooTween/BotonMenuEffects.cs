using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class BotonMenuEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI textoBoton;
    [SerializeField] private Image efectoHalo;
    [SerializeField] private RectMask2D haloMask;

    [Header("Efectos Visuales - Escala")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.03f, 1.03f, 1.03f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.02f, 1.02f, 1.02f);
    [SerializeField] private Vector3 escalaClick = new Vector3(0.98f, 0.98f, 0.98f);
    [SerializeField] private float duracionAnimacion = 0.25f;
    [SerializeField] private Ease tipoEase = Ease.OutCubic;

    [Header("Colores del Texto")]
    [SerializeField] private Color colorNormal = new Color(0.91f, 0.89f, 0.83f, 0.7f);
    [SerializeField] private Color colorHover = new Color(0.051f, 0.106f, 0.165f, 1f);
    [SerializeField] private Color colorSeleccionado = new Color(0.051f, 0.106f, 0.165f, 1f);

    [Header("Efecto Halo")]
    [SerializeField] private bool usarEfectoHalo = true;
    [SerializeField] private float duracionHalo = 0.5f;
    [SerializeField] private float alphaHaloMax = 1f;
    [SerializeField] private Ease easeHalo = Ease.OutQuad;

    [Header("Efectos Adicionales")]
    [SerializeField] private bool efectoPulso = true;
    [SerializeField] private float intensidadPulso = 0.015f;

    private Vector3 escalaOriginal;
    private Button boton;
    private bool estaEnHover = false;
    private bool estaSeleccionado = false;
    private RectTransform maskRect;
    private RectTransform haloRect;
    private float anchoMaskOriginal;

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
            haloRect = efectoHalo.GetComponent<RectTransform>();
            efectoHalo.raycastTarget = false;
            
            Color c = efectoHalo.color;
            c.a = 0;
            efectoHalo.color = c;
            
            if (haloMask == null)
                haloMask = efectoHalo.GetComponentInParent<RectMask2D>();
            
            if (haloMask != null)
            {
                maskRect = haloMask.GetComponent<RectTransform>();
                anchoMaskOriginal = maskRect.rect.width;
            }
            
            efectoHalo.gameObject.SetActive(false);
        }

        Image fondo = GetComponent<Image>();
        if (fondo != null)
            fondo.color = new Color(1, 1, 1, 0);
    }

    private void Start()
    {
        escalaOriginal = transform.localScale;
        if (textoBoton != null)
            textoBoton.color = colorNormal;
    }

    private void OnEnable()
    {
        // Forzar reseteo al activarse
        StartCoroutine(ResetearConDelay());
    }

    private System.Collections.IEnumerator ResetearConDelay()
    {
        // Esperar un frame para que Unity termine de inicializar todo
        yield return null;
        
        // Forzar deselección en el EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        // Resetear estado completamente
        ResetearEstado();
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColor?.Kill();
        tweenPulso?.Kill();
        secuenciaHalo?.Kill();
    }

    // ==================== MÉTODO PÚBLICO PARA RESETEAR ====================
    /// <summary>
    /// Resetea el estado del botón a su estado normal.
    /// Llama a este método cuando regreses al menú para limpiar la selección.
    /// </summary>
    public void ResetearEstado()
    {
        estaSeleccionado = false;
        estaEnHover = false;
        
        DetenerPulso();
        secuenciaHalo?.Kill();
        tweenEscala?.Kill();
        tweenColor?.Kill();
        
        // Restaurar a estado normal
        transform.localScale = escalaOriginal;
        
        if (textoBoton != null)
            textoBoton.color = colorNormal;
        
        if (efectoHalo != null)
        {
            efectoHalo.gameObject.SetActive(false);
            Color c = efectoHalo.color;
            c.a = 0;
            efectoHalo.color = c;
        }
        
        // Resetear máscara si existe
        if (maskRect != null)
        {
            RectMask2D mask = maskRect.GetComponent<RectMask2D>();
            Vector4 padding = mask.padding;
            padding.x = 0;
            padding.z = anchoMaskOriginal;
            mask.padding = padding;
        }
        
        // Deseleccionar en el EventSystem
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHaloConBarrido(true);
            else
                CambiarColorTexto(colorHover);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            if (!estaSeleccionado)
            {
                if (efectoHalo != null)
                    MostrarHaloConBarrido(false);
                else
                    CambiarColorTexto(colorNormal);
            }
        }
    }

    private void AnimarSeleccion(bool seleccionar)
    {
        if (seleccionar)
        {
            AnimarEscala(escalaSeleccionado);
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHaloConBarrido(true);
            else
                CambiarColorTexto(colorSeleccionado);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            if (!estaEnHover)
            {
                if (efectoHalo != null)
                    MostrarHaloConBarrido(false);
                else
                    CambiarColorTexto(colorNormal);
            }
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

    // ==================== HALO CON BARRIDO ====================
    private void MostrarHaloConBarrido(bool mostrar)
    {
        if (efectoHalo == null) return;

        secuenciaHalo?.Kill();
        tweenColor?.Kill();

        if (mostrar)
        {
            efectoHalo.gameObject.SetActive(true);

            Color c = efectoHalo.color;
            c.a = alphaHaloMax;
            efectoHalo.color = c;

            secuenciaHalo = DOTween.Sequence();

            if (maskRect != null)
            {
                RectMask2D mask = maskRect.GetComponent<RectMask2D>();
                
                Vector4 padding = mask.padding;
                padding.x = 0;
                padding.z = anchoMaskOriginal;
                mask.padding = padding;

                secuenciaHalo.Append(
                    DOTween.To(
                        () => mask.padding.z,
                        x => {
                            Vector4 p = mask.padding;
                            p.z = x;
                            mask.padding = p;
                        },
                        0f,
                        duracionHalo
                    ).SetEase(easeHalo)
                );
            }
            else if (haloRect != null)
            {
                Vector2 anchoredPos = haloRect.anchoredPosition;
                anchoredPos.x = -haloRect.rect.width;
                haloRect.anchoredPosition = anchoredPos;
                
                secuenciaHalo.Append(
                    haloRect.DOAnchorPosX(0, duracionHalo)
                        .SetEase(easeHalo)
                );
            }

            if (textoBoton != null)
            {
                secuenciaHalo.Join(
                    textoBoton.DOColor(colorHover, duracionHalo)
                        .SetEase(easeHalo)
                );
            }

            secuenciaHalo.SetUpdate(true);
        }
        else
        {
            if (!estaSeleccionado && !estaEnHover)
            {
                secuenciaHalo = DOTween.Sequence();

                secuenciaHalo.Join(
                    efectoHalo.DOFade(0f, duracionHalo * 0.5f)
                        .SetEase(Ease.InQuad)
                );

                if (textoBoton != null)
                {
                    secuenciaHalo.Join(
                        textoBoton.DOColor(colorNormal, duracionHalo * 0.5f)
                            .SetEase(Ease.InQuad)
                    );
                }

                if (maskRect != null)
                {
                    RectMask2D mask = maskRect.GetComponent<RectMask2D>();
                    secuenciaHalo.Join(
                        DOTween.To(
                            () => mask.padding.z,
                            x => {
                                Vector4 p = mask.padding;
                                p.z = x;
                                mask.padding = p;
                            },
                            anchoMaskOriginal,
                            duracionHalo * 0.5f
                        ).SetEase(Ease.InQuad)
                    );
                }

                secuenciaHalo.SetUpdate(true)
                    .OnComplete(() => efectoHalo.gameObject.SetActive(false));
            }
        }
    }

    private void MostrarHaloCompletoSinBarrido()
    {
        if (efectoHalo == null) return;

        secuenciaHalo?.Kill();
        tweenColor?.Kill();
        
        efectoHalo.gameObject.SetActive(true);

        if (maskRect != null)
        {
            RectMask2D mask = maskRect.GetComponent<RectMask2D>();
            Vector4 padding = mask.padding;
            padding.x = 0;
            padding.z = 0;
            mask.padding = padding;
        }

        if (haloRect != null)
        {
            Vector2 anchoredPos = haloRect.anchoredPosition;
            anchoredPos.x = 0;
            haloRect.anchoredPosition = anchoredPos;
        }

        Color c = efectoHalo.color;
        c.a = alphaHaloMax;
        efectoHalo.DOColor(c, duracionHalo * 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
        
        if (textoBoton != null)
        {
            textoBoton.DOColor(colorSeleccionado, duracionHalo * 0.4f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
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

        if (usarEfectoHalo && efectoHalo != null && !efectoHalo.gameObject.activeSelf)
            MostrarHaloCompletoSinBarrido();
    }

    private void DetenerPulso()
    {
        tweenPulso?.Kill();
        if (estaSeleccionado)
            transform.DOScale(escalaSeleccionado, duracionAnimacion * 0.5f).SetUpdate(true);
    }
}