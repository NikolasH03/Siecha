using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class BotonMenuPausa : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Referencias")]
    [SerializeField] private Image imagenBoton; // La imagen del ícono del botón (ASIGNAR MANUALMENTE SI HAY DUPLICADOS)
    [SerializeField] private TextMeshProUGUI textoNombre; // El texto del botón que contiene su nombre
    [SerializeField] private GameObject objetoTextoGlobal; // El GameObject del texto que se muestra arriba (ej: "RESTART")
    [SerializeField] private bool mostrarTodasLasImagenes = false; // Debug: muestra todas las imágenes encontradas
    
    [Header("Opacidad")]
    [SerializeField] private float opacidadInicial = 0.6f;
    [SerializeField] private float opacidadActiva = 1f;
    [SerializeField] private float duracionOpacidad = 0.3f;
    
    [Header("Escala")]
    [SerializeField] private Vector3 escalaHover = new Vector3(1.02f, 1.02f, 1.02f); // Aún más sutil
    [SerializeField] private float duracionEscala = 0.25f;
    [SerializeField] private Ease easeEscala = Ease.OutCubic;
    [SerializeField] private bool forzarEscalaUniforme = false; // Desactivado por defecto
    [SerializeField] private bool usarEscalaRelativa = true; // Usa multiplicador en vez de valor absoluto
    
    [Header("Efecto Pulso (cuando está seleccionado)")]
    [SerializeField] private bool usarPulso = true;
    [SerializeField] private float intensidadPulso = 0.02f; // Pulso sutil
    [SerializeField] private float duracionPulso = 1.2f;
    
    [Header("Animación del Texto Global")]
    [SerializeField] private float duracionTexto = 0.35f;
    [SerializeField] private float desplazamientoTexto = 15f;
    [SerializeField] private bool esBotonPorDefecto = false; // Marca el botón que debe mostrarse al inicio (ej: RESTART)
    
    private Vector3 escalaOriginalImagen;
    private Button boton;
    private CanvasGroup canvasGroupImagen; // CanvasGroup de la imagen, no del padre
    private TextMeshProUGUI textoGlobal; // Referencia al componente Text del objeto global
    
    private bool estaEnHover = false;
    private bool estaSeleccionado = false;
    
    private Tween tweenOpacidad;
    private Tween tweenEscala;
    private Tween tweenPulso;
    private static Sequence secuenciaTextoActual; // Static para que solo haya una animación a la vez

    public string textoActualMostrado { get; private set; }

    private void Awake()
    {
        boton = GetComponent<Button>();
        
        // Si no se asignó la imagen manualmente, buscarla
        if (imagenBoton == null)
        {
            // Buscar solo imágenes activas para evitar tomar duplicados ocultos
            Image[] imagenes = GetComponentsInChildren<Image>(false); // false = solo activas
            
            if (mostrarTodasLasImagenes)
            {
                Debug.Log($"[{gameObject.name}] ========== TODAS LAS IMÁGENES ENCONTRADAS ==========");
                foreach (Image img in imagenes)
                {
                    Debug.Log($"  - {img.gameObject.name} (Sprite: {img.sprite?.name ?? "null"})");
                }
                Debug.Log($"[{gameObject.name}] ===================================================");
            }
            
            // Filtrar: no tomar la imagen del botón padre, ni textos
            foreach (Image img in imagenes)
            {
                // Saltar si es el componente del mismo GameObject del botón
                if (img.gameObject == gameObject) continue;
                
                // Saltar si tiene TextMeshProUGUI (es un texto, no un ícono)
                if (img.GetComponent<TextMeshProUGUI>() != null) continue;
                
                // Esta debe ser la imagen del ícono
                imagenBoton = img;
                Debug.Log($"[{gameObject.name}] ✅ Imagen detectada automáticamente: {img.gameObject.name} (Sprite: {img.sprite?.name ?? "null"})");
                break;
            }
            
            // Si aún no encuentra, advertir
            if (imagenBoton == null)
            {
                Debug.LogError($"[{gameObject.name}] ⚠️ NO SE ENCONTRÓ IMAGEN. Asigna manualmente 'Imagen Boton' en el Inspector.");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] ✅ Usando imagen asignada manualmente: {imagenBoton.gameObject.name} (Sprite: {imagenBoton.sprite?.name ?? "null"})");
        }
        
        // Obtener o crear CanvasGroup SOLO para la imagen
        if (imagenBoton != null)
        {
            canvasGroupImagen = imagenBoton.GetComponent<CanvasGroup>();
            if (canvasGroupImagen == null)
            {
                canvasGroupImagen = imagenBoton.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Obtener el componente TextMeshProUGUI del objeto global
        if (objetoTextoGlobal != null)
        {
            textoGlobal = objetoTextoGlobal.GetComponent<TextMeshProUGUI>();
        }
    }
    
    private void Start()
    {
        // Guardar la escala original de la IMAGEN, no del padre
        if (imagenBoton != null)
        {
            escalaOriginalImagen = imagenBoton.transform.localScale;
            
            // Debug para verificar qué imagen se está usando
            Debug.Log($"[{gameObject.name}] Imagen asignada: {imagenBoton.gameObject.name}, Escala original: {escalaOriginalImagen}");
            
            // Forzar escala uniforme si está activado
            if (forzarEscalaUniforme)
            {
                float escalaPromedio = (escalaOriginalImagen.x + escalaOriginalImagen.y + escalaOriginalImagen.z) / 3f;
                escalaOriginalImagen = Vector3.one * escalaPromedio;
                imagenBoton.transform.localScale = escalaOriginalImagen;
                Debug.Log($"[{gameObject.name}] Escala normalizada a: {escalaOriginalImagen}");
            }
            
            // IMPORTANTE: Asegurar que la imagen empiece en escala original y opacidad correcta
            imagenBoton.transform.localScale = escalaOriginalImagen;
            
            // Establecer estado inicial: 60% de opacidad en la imagen
            if (canvasGroupImagen != null)
            {
                canvasGroupImagen.alpha = opacidadInicial;
            }
        }
        
        // Forzar estados iniciales
        estaSeleccionado = false;
        estaEnHover = false;
        
        // Gestión inicial del TEXTO global (NO afecta el estado visual del botón)
        if (objetoTextoGlobal != null && textoGlobal != null)
        {
            if (esBotonPorDefecto)
            {
                textoActualMostrado = textoNombre != null ? textoNombre.text : "";
                objetoTextoGlobal.SetActive(true);
                textoGlobal.text = textoActualMostrado;
                textoGlobal.alpha = 1f;

                // Forzar animación visual de selección
                estaSeleccionado = true;
                AnimarEstadoActivo(true);
                if (usarPulso) IniciarPulso();

                Debug.Log($"[{gameObject.name}] Configurado como botón por defecto con animaciones activas.");
            }
        }
        
        // CRÍTICO: Forzar deselección DESPUÉS de configurar estados visuales
        // Esto previene que Unity auto-seleccione el botón y dispare OnSelect()
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log($"[{gameObject.name}] EventSystem deseleccionado para evitar doble activación.");
        }
    }
    
    private void OnDestroy()
    {
        // Limpiar todos los tweens
        tweenOpacidad?.Kill();
        tweenEscala?.Kill();
        tweenPulso?.Kill();
    }
    
    // ==================== EVENTOS ====================
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        
        estaEnHover = true;
        
        if (!estaSeleccionado)
        {
            AnimarEstadoActivo(true);
            // Cambiar el texto al hacer hover
            MostrarTextoBoton();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (boton && !boton.interactable) return;
        
        estaEnHover = false;
        
        if (!estaSeleccionado)
        {
            AnimarEstadoActivo(false);
        }
        
        // Siempre ocultar el texto al salir del hover
        OcultarTextoBoton();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (boton && !boton.interactable) return;
        
        Debug.Log($"[{gameObject.name}] OnSelect llamado");
        
        estaSeleccionado = true;
        
        AnimarEstadoActivo(true);
        
        // Mostrar el texto del botón seleccionado
        MostrarTextoBoton();
        
        // Iniciar efecto de pulso
        if (usarPulso)
        {
            IniciarPulso();
        }
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        estaSeleccionado = false;
        
        // Detener el pulso
        DetenerPulso();
        
        // Si el mouse sigue sobre el botón, mantener estado hover
        if (estaEnHover)
        {
            AnimarEstadoActivo(true);
        }
        else
        {
            AnimarEstadoActivo(false);
            OcultarTextoBoton();
        }
    }
    
    // ==================== ANIMACIONES ====================
    
    private void AnimarEstadoActivo(bool activar)
    {
        if (activar)
        {
            // Estado activo: 100% opacidad, escala aumentada EN LA IMAGEN
            AnimarOpacidad(opacidadActiva);
            AnimarEscala(escalaHover);
        }
        else
        {
            // Estado inicial: 60% opacidad, escala original EN LA IMAGEN
            AnimarOpacidad(opacidadInicial);
            AnimarEscala(escalaOriginalImagen);
        }
    }
    
    private void AnimarOpacidad(float objetivo)
    {
        if (canvasGroupImagen == null) return;
        
        tweenOpacidad?.Kill();
        tweenOpacidad = canvasGroupImagen.DOFade(objetivo, duracionOpacidad)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }
    
    private void AnimarEscala(Vector3 objetivo)
    {
        if (imagenBoton == null) return;
        
        Vector3 escalaFinal;
        
        if (usarEscalaRelativa)
        {
            // Usar escala RELATIVA (multiplicador sobre la escala original)
            if (objetivo == escalaHover)
            {
                // Aplicar el factor 1.02 sobre la escala original
                escalaFinal = new Vector3(
                    escalaOriginalImagen.x * 1.02f,
                    escalaOriginalImagen.y * 1.02f,
                    escalaOriginalImagen.z * 1.02f
                );
            }
            else if (objetivo == escalaOriginalImagen + Vector3.one * intensidadPulso)
            {
                // Para pulso
                escalaFinal = new Vector3(
                    escalaOriginalImagen.x * 1.04f, // 1.02 + 0.02 del pulso
                    escalaOriginalImagen.y * 1.04f,
                    escalaOriginalImagen.z * 1.04f
                );
            }
            else
            {
                // Volver a la escala original
                escalaFinal = escalaOriginalImagen;
            }
        }
        else
        {
            // Usar escala ABSOLUTA (método antiguo)
            if (objetivo == escalaHover)
            {
                float factorEscala = escalaHover.x;
                escalaFinal = escalaOriginalImagen * factorEscala;
            }
            else if (objetivo == escalaOriginalImagen + Vector3.one * intensidadPulso)
            {
                float factorEscala = escalaHover.x + intensidadPulso;
                escalaFinal = escalaOriginalImagen * factorEscala;
            }
            else
            {
                escalaFinal = escalaOriginalImagen;
            }
        }
        
        tweenEscala?.Kill();
        tweenEscala = imagenBoton.transform.DOScale(escalaFinal, duracionEscala)
            .SetEase(easeEscala)
            .SetUpdate(true);
    }
    
    // ==================== PULSO ====================
    
    private void IniciarPulso()
    {
        if (!usarPulso || imagenBoton == null) return;
        
        tweenPulso?.Kill();
        
        // Crear un pulso sutil que va desde la escala hover hasta un poco más grande
        Vector3 escalaPulso = escalaHover + Vector3.one * intensidadPulso;
        
        tweenPulso = imagenBoton.transform.DOScale(escalaPulso, duracionPulso)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }
    
    private void DetenerPulso()
    {
        tweenPulso?.Kill();
        
        // Volver a la escala correspondiente suavemente
        if (estaSeleccionado || estaEnHover)
        {
            AnimarEscala(escalaHover);
        }
        else
        {
            AnimarEscala(escalaOriginalImagen);
        }
    }
    
    // ==================== TEXTO ====================
    
    private void MostrarTextoBoton()
    {
        if (textoNombre == null || objetoTextoGlobal == null || textoGlobal == null) return;
        
        // Obtener el nombre del texto de este botón
        string nuevoTexto = textoNombre.text;
        
        // CRÍTICO: Cancelar INMEDIATAMENTE cualquier animación en curso
        secuenciaTextoActual?.Kill(true); // El 'true' completa la animación actual instantáneamente
        
        // Si el texto ya es el correcto y está visible, no hacer nada
        if (objetoTextoGlobal.activeSelf && textoGlobal.text == nuevoTexto && textoGlobal.alpha >= 0.99f)
        {
            return;
        }
        
        // Activar el objeto si estaba inactivo
        if (!objetoTextoGlobal.activeSelf)
        {
            objetoTextoGlobal.SetActive(true);
            textoGlobal.alpha = 0f;
        }
        
        // Si el texto es diferente, cambiar INMEDIATAMENTE sin animación de por medio
        if (textoGlobal.text != nuevoTexto)
        {
            // Cambio instantáneo de texto
            float alphaActual = textoGlobal.alpha;
            textoGlobal.text = nuevoTexto;
            textoGlobal.alpha = alphaActual; // Mantener el alpha actual
            
            // Solo hacer fade in si está muy transparente
            if (alphaActual < 0.5f)
            {
                secuenciaTextoActual = DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(textoGlobal.DOFade(1f, duracionTexto * 0.4f).SetEase(Ease.OutCubic));
            }
            else
            {
                // Ya está visible, solo asegurar alpha completo
                textoGlobal.alpha = 1f;
            }
        }
        else
        {
            // Mismo texto, solo asegurar que esté visible
            secuenciaTextoActual = DOTween.Sequence()
                .SetUpdate(true)
                .Append(textoGlobal.DOFade(1f, duracionTexto * 0.3f).SetEase(Ease.OutCubic));
        }
    }
    
    private void OcultarTextoBoton()
    {
        if (objetoTextoGlobal == null || textoGlobal == null) return;
        if (!objetoTextoGlobal.activeSelf) return;
        
        // Cancelar animación en curso
        secuenciaTextoActual?.Kill();
        
        // Animación de salida (solo fade, sin movimiento)
        secuenciaTextoActual = DOTween.Sequence()
            .SetUpdate(true)
            .Append(textoGlobal.DOFade(0f, duracionTexto * 0.5f).SetEase(Ease.InCubic))
            .OnComplete(() => 
            {
                objetoTextoGlobal.SetActive(false);
            });
    }
}