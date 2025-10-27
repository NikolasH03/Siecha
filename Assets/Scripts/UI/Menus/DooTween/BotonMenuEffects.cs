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
    [SerializeField] private RectMask2D haloMask; // La máscara

    [Header("Efectos Visuales - Escala")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.03f, 1.03f, 1.03f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.02f, 1.02f, 1.02f);
    [SerializeField] private Vector3 escalaClick = new Vector3(0.98f, 0.98f, 0.98f);
    [SerializeField] private float duracionAnimacion = 0.25f;
    [SerializeField] private Ease tipoEase = Ease.OutCubic;

    [Header("Colores del Texto")]
    [SerializeField] private Color colorNormal = new Color(0.91f, 0.89f, 0.83f, 0.7f); // E8E3D3 70%
    [SerializeField] private Color colorHover = new Color(0.051f, 0.106f, 0.165f, 1f); // 0D1B2A 100%
    [SerializeField] private Color colorSeleccionado = new Color(0.051f, 0.106f, 0.165f, 1f); // 0D1B2A 100%

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

        // Configurar halo
        if (efectoHalo != null)
        {
            haloRect = efectoHalo.GetComponent<RectTransform>();
            efectoHalo.raycastTarget = false;
            
            Color c = efectoHalo.color;
            c.a = 0;
            efectoHalo.color = c;
            
            // Buscar o crear la máscara
            if (haloMask == null)
                haloMask = efectoHalo.GetComponentInParent<RectMask2D>();
            
            if (haloMask != null)
            {
                maskRect = haloMask.GetComponent<RectTransform>();
                anchoMaskOriginal = maskRect.rect.width;
            }
            
            efectoHalo.gameObject.SetActive(false);
        }

        // Fondo transparente
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
            // Animar halo CON cambio de color sincronizado
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHaloConBarrido(true);
            else
                // Si no hay halo, cambiar color directamente
                CambiarColorTexto(colorHover);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            // Solo cambiar a normal si no está seleccionado
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
            // Reproduce la misma animación de barrido que el hover
            if (usarEfectoHalo && efectoHalo != null)
                MostrarHaloConBarrido(true); // ✅ animación de entrada
            else
                CambiarColorTexto(colorSeleccionado);
        }
        else
        {
            AnimarEscala(escalaOriginal);
            // Solo cambiar a normal si no está en hover
            if (!estaEnHover)
            {
                if (efectoHalo != null)
                    MostrarHaloConBarrido(false); // ✅ animación de salida
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
        tweenColor?.Kill(); // Cancelar animación de color previa

        if (mostrar)
        {
            // Activar halo
            efectoHalo.gameObject.SetActive(true);

            // Alpha visible
            Color c = efectoHalo.color;
            c.a = alphaHaloMax;
            efectoHalo.color = c;

            secuenciaHalo = DOTween.Sequence();

            // Si tiene máscara, configurar para revelar de izquierda a derecha
            if (maskRect != null)
            {
                RectMask2D mask = maskRect.GetComponent<RectMask2D>();
                
                // INICIO: padding derecho = ancho completo (oculta todo desde la derecha)
                Vector4 padding = mask.padding;
                padding.x = 0; // Sin padding izquierdo
                padding.z = anchoMaskOriginal; // Padding derecho = todo oculto
                mask.padding = padding;

                // ANIMACIÓN: reducir padding derecho de ancho completo a 0 (revelar de izquierda a derecha)
                secuenciaHalo.Append(
                    DOTween.To(
                        () => mask.padding.z, // Animar padding DERECHO
                        x => {
                            Vector4 p = mask.padding;
                            p.z = x;
                            mask.padding = p;
                        },
                        0f, // Reducir a 0 = completamente visible
                        duracionHalo
                    ).SetEase(easeHalo)
                );
            }
            else if (haloRect != null)
            {
                // Si no hay máscara, animar la posición del halo
                Vector2 anchoredPos = haloRect.anchoredPosition;
                anchoredPos.x = -haloRect.rect.width;
                haloRect.anchoredPosition = anchoredPos;
                
                secuenciaHalo.Append(
                    haloRect.DOAnchorPosX(0, duracionHalo)
                        .SetEase(easeHalo)
                );
            }

            // SINCRONIZAR: Cambiar color del texto JUNTO con el barrido
            if (textoBoton != null)
            {
                Debug.Log($"Color Normal: {colorNormal} | Color Hover: {colorHover}");
                Debug.Log($"Color actual del texto antes de animar: {textoBoton.color}");
                
                secuenciaHalo.Join(
                    textoBoton.DOColor(colorHover, duracionHalo)
                        .SetEase(easeHalo)
                );
            }

            secuenciaHalo.SetUpdate(true)
                .OnComplete(() => {
                    if (textoBoton != null)
                        Debug.Log($"Barrido completo - Color final: RGBA({textoBoton.color.r:F3}, {textoBoton.color.g:F3}, {textoBoton.color.b:F3}, {textoBoton.color.a:F3})");
                });
        }
        else
        {
            // Solo ocultar si no hay hover ni selección
            if (!estaSeleccionado && !estaEnHover)
            {
                secuenciaHalo = DOTween.Sequence();

                // Fade out del halo
                secuenciaHalo.Join(
                    efectoHalo.DOFade(0f, duracionHalo * 0.5f)
                        .SetEase(Ease.InQuad)
                );

                // Cambiar texto de vuelta a color normal
                if (textoBoton != null)
                {
                    secuenciaHalo.Join(
                        textoBoton.DOColor(colorNormal, duracionHalo * 0.5f)
                            .SetEase(Ease.InQuad)
                    );
                }

                // Resetear máscara (volver a ocultar desde la derecha)
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
                            anchoMaskOriginal, // Volver a ocultar
                            duracionHalo * 0.5f
                        ).SetEase(Ease.InQuad)
                    );
                }

                secuenciaHalo.SetUpdate(true)
                    .OnComplete(() => efectoHalo.gameObject.SetActive(false));
            }
        }
    }

    // Mostrar halo completo sin animación de barrido (para selección)
    private void MostrarHaloCompletoSinBarrido()
    {
        if (efectoHalo == null) return;

        secuenciaHalo?.Kill();
        tweenColor?.Kill();
        
        efectoHalo.gameObject.SetActive(true);

        // Máscara revelada completamente (sin padding)
        if (maskRect != null)
        {
            RectMask2D mask = maskRect.GetComponent<RectMask2D>();
            Vector4 padding = mask.padding;
            padding.x = 0; // Sin padding izquierdo
            padding.z = 0; // Sin padding derecho = completamente visible
            mask.padding = padding;
        }

        // Halo en posición correcta
        if (haloRect != null)
        {
            Vector2 anchoredPos = haloRect.anchoredPosition;
            anchoredPos.x = 0;
            haloRect.anchoredPosition = anchoredPos;
        }

        // Fade in suave del halo
        Color c = efectoHalo.color;
        c.a = alphaHaloMax;
        efectoHalo.DOColor(c, duracionHalo * 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
        
        // Cambiar color del texto
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

        // Asegurar que el halo esté visible
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