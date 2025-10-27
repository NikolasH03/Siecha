using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class ComboNavigator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentParent; // El Content que contiene los combos
    
    [Header("Navegación Gamepad")]
    [SerializeField] private string inputVertical = "Vertical"; 
    [SerializeField] private float cooldownInput = 0.15f; 
    [SerializeField] private float repeatDelay = 0.4f; 
    
    [Header("Efectos Visuales")]
    [SerializeField] private float escalaSeleccionado = 1.125f; 
    [SerializeField] private float escalaNormal = 1f;
    [SerializeField] private float duracionEscala = 0.25f;
    [SerializeField] private Ease easeEscala = Ease.OutBack;
    
    [Header("Efecto Pulso")]
    [SerializeField] private bool usarPulso = true;
    [SerializeField] private float intensidadPulso = 0.025f; 
    [SerializeField] private float velocidadPulso = 1f;
    
    [Header("Desplazamiento Suave")]
    [SerializeField] private float duracionScroll = 0.4f;
    [SerializeField] private Ease easeScroll = Ease.OutQuad;
    
    private List<RectTransform> combos = new List<RectTransform>();
    private int indiceActual = 0;
    private float tiempoUltimoInput = 0f;
    private float tiempoPresionado = 0f;
    private bool teclaPresionada = false;
    private int direccionActual = 0;
    private Tween tweenPulso;
    private Tween tweenScroll;

    private void Awake()
    {
        // Si no se asignó el ScrollRect, buscar en este objeto
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        
        // Si no se asignó el content, usar el del ScrollRect
        if (contentParent == null && scrollRect != null)
            contentParent = scrollRect.content;
    }

    private void Start()
    {
        InicializarCombos();
        
        if (combos.Count > 0)
        {
            SeleccionarCombo(0, false); // Seleccionar el primero sin animación
        }
    }

    private void OnEnable()
    {
        // Resetear al activarse
        if (combos.Count > 0)
        {
            indiceActual = 0;
            SeleccionarCombo(0, false);
        }
    }

    private void OnDisable()
    {
        // Limpiar animaciones al desactivar
        tweenPulso?.Kill();
        tweenScroll?.Kill();
        
        // Resetear escalas
        foreach (var combo in combos)
        {
            if (combo != null)
                combo.localScale = Vector3.one * escalaNormal;
        }
    }

    private void Update()
    {
        if (combos.Count == 0) return;
        
        // Solo detectar input de gamepad (no teclado)
        float inputAxis = Input.GetAxisRaw(inputVertical);
        bool gamepadArriba = inputAxis > 0.1f;
        bool gamepadAbajo = inputAxis < -0.1f;
        
        int nuevaDireccion = 0;
        if (gamepadArriba) nuevaDireccion = -1;
        else if (gamepadAbajo) nuevaDireccion = 1;
        
        // Si está usando el gamepad
        if (nuevaDireccion != 0)
        {
            // Si es una nueva pulsación
            if (!teclaPresionada || nuevaDireccion != direccionActual)
            {
                Navegar(nuevaDireccion);
                teclaPresionada = true;
                direccionActual = nuevaDireccion;
                tiempoPresionado = Time.unscaledTime;
                tiempoUltimoInput = Time.unscaledTime;
            }
            // Si mantiene presionada
            else
            {
                float tiempoManteniendoTecla = Time.unscaledTime - tiempoPresionado;
                
                // Después del delay inicial, permitir repetición rápida
                if (tiempoManteniendoTecla > repeatDelay)
                {
                    if (Time.unscaledTime - tiempoUltimoInput >= cooldownInput)
                    {
                        Navegar(direccionActual);
                        tiempoUltimoInput = Time.unscaledTime;
                    }
                }
            }
        }
        else
        {
            // Stick soltado
            teclaPresionada = false;
            direccionActual = 0;
            tiempoPresionado = 0f;
        }
    }

    private void InicializarCombos()
    {
        combos.Clear();
        
        if (contentParent == null)
        {
            Debug.LogError("ComboNavigator: No se asignó el Content Parent");
            return;
        }
        
        // Obtener todos los hijos directos que sean combos
        for (int i = 0; i < contentParent.childCount; i++)
        {
            Transform child = contentParent.GetChild(i);
            
            // Solo agregar si está activo
            if (child.gameObject.activeSelf)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null)
                {
                    combos.Add(rt);
                    // Asegurar escala inicial
                    rt.localScale = Vector3.one * escalaNormal;
                    
                    // Agregar componente para detectar hover
                    AgregarDetectorHover(rt.gameObject, i);
                }
            }
        }
        
        Debug.Log($"ComboNavigator: Se encontraron {combos.Count} combos");
    }

    private void AgregarDetectorHover(GameObject combo, int indice)
    {
        // Remover detector previo si existe
        ComboHoverDetector detector = combo.GetComponent<ComboHoverDetector>();
        if (detector != null)
            Destroy(detector);
        
        // Agregar nuevo detector
        detector = combo.AddComponent<ComboHoverDetector>();
        detector.Inicializar(this, indice);
    }

    private void Navegar(int direccion)
    {
        if (combos.Count == 0) return;
        
        int nuevoIndice = indiceActual + direccion;
        
        // Hacer wrap (ciclar)
        if (nuevoIndice < 0)
            nuevoIndice = combos.Count - 1;
        else if (nuevoIndice >= combos.Count)
            nuevoIndice = 0;
        
        SeleccionarCombo(nuevoIndice, true);
    }

    private void SeleccionarCombo(int indice, bool conAnimacion)
    {
        if (indice < 0 || indice >= combos.Count) return;
        
        // Deseleccionar el anterior
        if (indiceActual >= 0 && indiceActual < combos.Count)
        {
            DeseleccionarCombo(combos[indiceActual], conAnimacion);
        }
        
        // Seleccionar el nuevo
        indiceActual = indice;
        RectTransform comboSeleccionado = combos[indiceActual];
        
        if (comboSeleccionado != null)
        {
            // Aplicar escala
            if (conAnimacion)
            {
                comboSeleccionado.DOScale(escalaSeleccionado, duracionEscala)
                    .SetEase(easeEscala)
                    .SetUpdate(true);
            }
            else
            {
                comboSeleccionado.localScale = Vector3.one * escalaSeleccionado;
            }
            
            // Iniciar pulso
            if (usarPulso)
                IniciarPulso(comboSeleccionado);
            
            // Hacer scroll hacia el combo
            if (scrollRect != null && conAnimacion)
                ScrollHaciaCombo(comboSeleccionado);
        }
    }

    private void DeseleccionarCombo(RectTransform combo, bool conAnimacion)
    {
        if (combo == null) return;
        
        // Detener pulso
        tweenPulso?.Kill();
        
        // Volver a escala normal
        if (conAnimacion)
        {
            combo.DOScale(escalaNormal, duracionEscala * 0.7f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
        else
        {
            combo.localScale = Vector3.one * escalaNormal;
        }
    }

    private void IniciarPulso(RectTransform combo)
    {
        if (!usarPulso || combo == null) return;
        
        tweenPulso?.Kill();
        
        float escalaMin = escalaSeleccionado;
        float escalaMax = escalaSeleccionado + intensidadPulso;
        
        tweenPulso = combo.DOScale(escalaMax, velocidadPulso)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void ScrollHaciaCombo(RectTransform combo)
    {
        if (scrollRect == null || contentParent == null) return;
        
        tweenScroll?.Kill();
        
        // Calcular la posición normalizada del combo
        Canvas.ForceUpdateCanvases();
        
        Vector2 viewportSize = scrollRect.viewport.rect.size;
        Vector2 contentSize = contentParent.rect.size;
        
        // Posición del combo relativa al content
        float comboY = -combo.anchoredPosition.y;
        float comboAltura = combo.rect.height;
        
        // Calcular posición para centrar el combo
        float targetY = comboY - (viewportSize.y / 2f) + (comboAltura / 2f);
        
        // Normalizar (0 = arriba, 1 = abajo)
        float scrollableHeight = contentSize.y - viewportSize.y;
        float normalizedY = Mathf.Clamp01(targetY / scrollableHeight);
        
        // Invertir porque Unity usa 1 = arriba, 0 = abajo
        normalizedY = 1f - normalizedY;
        
        // Animar scroll
        tweenScroll = DOTween.To(
            () => scrollRect.verticalNormalizedPosition,
            x => scrollRect.verticalNormalizedPosition = x,
            normalizedY,
            duracionScroll
        ).SetEase(easeScroll)
         .SetUpdate(true);
    }

    // ==================== MÉTODOS PÚBLICOS ====================
    
    /// <summary>
    /// Llamado cuando el mouse entra en un combo (desde ComboHoverDetector)
    /// </summary>
    public void OnComboHoverEnter(int indice)
    {
        if (indice >= 0 && indice < combos.Count && indice != indiceActual)
        {
            SeleccionarCombo(indice, true);
        }
    }

    /// <summary>
    /// Selecciona un combo específico por índice
    /// </summary>
    public void SeleccionarPorIndice(int indice)
    {
        if (indice >= 0 && indice < combos.Count)
        {
            SeleccionarCombo(indice, true);
        }
    }

    /// <summary>
    /// Obtiene el índice del combo actualmente seleccionado
    /// </summary>
    public int ObtenerIndiceActual()
    {
        return indiceActual;
    }

    /// <summary>
    /// Obtiene el GameObject del combo actualmente seleccionado
    /// </summary>
    public GameObject ObtenerComboSeleccionado()
    {
        if (indiceActual >= 0 && indiceActual < combos.Count)
            return combos[indiceActual].gameObject;
        return null;
    }

    /// <summary>
    /// Reinicializa la lista de combos (útil si se añaden/quitan dinámicamente)
    /// </summary>
    public void RefrescarCombos()
    {
        InicializarCombos();
        if (combos.Count > 0)
            SeleccionarCombo(0, false);
    }

    /// <summary>
    /// Resetea la navegación al primer combo
    /// </summary>
    public void ResetearNavegacion()
    {
        if (combos.Count > 0)
            SeleccionarCombo(0, true);
    }
}

// ==================== COMPONENTE AUXILIAR ====================

/// <summary>
/// Componente que detecta cuando el mouse entra en un combo
/// Se agrega automáticamente a cada combo
/// </summary>
public class ComboHoverDetector : MonoBehaviour, IPointerEnterHandler
{
    private ComboNavigator navegador;
    private int indice;

    public void Inicializar(ComboNavigator nav, int idx)
    {
        navegador = nav;
        indice = idx;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (navegador != null)
        {
            navegador.OnComboHoverEnter(indice);
        }
    }
}