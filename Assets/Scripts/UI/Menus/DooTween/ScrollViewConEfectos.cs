using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ScrollViewConEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Efectos Visuales")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.02f, 1.02f, 1f);
    [SerializeField] private Vector3 escalaSeleccionado = new Vector3(1.01f, 1.01f, 1f);
    [SerializeField] private float duracionAnimacion = 0.2f;
    [SerializeField] private Ease tipoEase = Ease.OutQuad;

    [Header("Colores ScrollBar")]
    [SerializeField] private Color colorScrollbarNormal = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color colorScrollbarHover = new Color(1f, 1f, 0f, 0.8f);
    [SerializeField] private Color colorScrollbarSeleccionado = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private bool usarEfectoColorScrollbar = true;

    [Header("Efectos del Content")]
    [SerializeField] private bool resaltarElementos = true;
    [SerializeField] private float intensidadResaltado = 1.1f;

    [Header("Efectos Adicionales")]
    [SerializeField] private GameObject efectoSeleccion;
    [SerializeField] private bool mostrarIndicadorScroll = true;
    [SerializeField] private GameObject indicadorScroll; // Flecha o indicador visual

    private Vector3 escalaOriginal;
    private ScrollRect scrollRect;
    private Scrollbar scrollbar;
    private Image imagenScrollbar;
    private bool estaSeleccionado = false;
    private Tween tweenEscala;
    private Tween tweenColorScrollbar;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        scrollRect = GetComponent<ScrollRect>();

        // Buscar scrollbar (vertical por defecto)
        if (scrollRect != null)
        {
            scrollbar = scrollRect.verticalScrollbar;
            if (scrollbar != null)
            {
                imagenScrollbar = scrollbar.GetComponent<Image>();
            }
        }

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);

        if (indicadorScroll)
            indicadorScroll.SetActive(false);
    }

    private void OnDestroy()
    {
        tweenEscala?.Kill();
        tweenColorScrollbar?.Kill();
    }

    private void Update()
    {
        // Mostrar/ocultar indicador de scroll si hay contenido que scrollear
        if (mostrarIndicadorScroll && indicadorScroll && scrollRect)
        {
            bool tieneScroll = scrollRect.content.rect.height > scrollRect.viewport.rect.height;
            if (indicadorScroll.activeSelf != (tieneScroll && estaSeleccionado))
            {
                indicadorScroll.SetActive(tieneScroll && estaSeleccionado);
            }
        }
    }

    // EVENTOS DE MOUSE
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scrollRect && !scrollRect.enabled) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaHover);
            if (usarEfectoColorScrollbar) CambiarColorScrollbar(colorScrollbarHover);
            if (resaltarElementos) ResaltarElementos(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scrollRect && !scrollRect.enabled) return;
        if (!estaSeleccionado)
        {
            AnimarEscala(escalaOriginal);
            if (usarEfectoColorScrollbar) CambiarColorScrollbar(colorScrollbarNormal);
            if (resaltarElementos) ResaltarElementos(false);
        }
    }

    // EVENTOS DE MANDO/TECLADO
    public void OnSelect(BaseEventData eventData)
    {
        if (scrollRect && !scrollRect.enabled) return;

        estaSeleccionado = true;
        AnimarEscala(escalaSeleccionado);
        if (usarEfectoColorScrollbar) CambiarColorScrollbar(colorScrollbarSeleccionado);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(true);

        if (resaltarElementos)
            ResaltarElementos(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        AnimarEscala(escalaOriginal);
        if (usarEfectoColorScrollbar) CambiarColorScrollbar(colorScrollbarNormal);

        if (efectoSeleccion)
            efectoSeleccion.SetActive(false);

        if (resaltarElementos)
            ResaltarElementos(false);
    }

    private void AnimarEscala(Vector3 escalaObjetivo)
    {
        tweenEscala?.Kill();
        tweenEscala = transform.DOScale(escalaObjetivo, duracionAnimacion)
            .SetEase(tipoEase)
            .SetUpdate(true);
    }

    private void CambiarColorScrollbar(Color colorObjetivo)
    {
        if (imagenScrollbar)
        {
            tweenColorScrollbar?.Kill();
            tweenColorScrollbar = imagenScrollbar.DOColor(colorObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }

    private void ResaltarElementos(bool resaltar)
    {
        if (!resaltarElementos || scrollRect?.content == null) return;

        float escalaObjetivo = resaltar ? intensidadResaltado : 1f;

        // Resaltar todos los elementos hijos del content
        for (int i = 0; i < scrollRect.content.childCount; i++)
        {
            var child = scrollRect.content.GetChild(i);
            child.DOScale(escalaObjetivo, duracionAnimacion)
                .SetEase(tipoEase)
                .SetUpdate(true);
        }
    }
}