using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MenuInicial : MenuBase
{
    [Header("UI del Menú Inicial")]
    [SerializeField] private TextMeshProUGUI textoPressAnyKey;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Efectos Visuales")]
    [SerializeField] private bool efectoParpadeoPtext = true;
    [SerializeField] private float duracionParpadeo = 1f;
    [SerializeField] private bool efectoFadeIn = true;
    [SerializeField] private float duracionFadeIn = 2f;

    private bool esperandoInput = false;
    private Tween tweenParpadeo;
    private Tween tweenFadeIn;

    protected override void OnMenuOpened()
    {
        esperandoInput = true;

        // Cambiar a input UI para detectar cualquier tecla
        InputJugador.instance?.CambiarInputUI();

        // Efectos visuales
        if (efectoFadeIn && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            tweenFadeIn = canvasGroup.DOFade(1f, duracionFadeIn)
                .SetUpdate(true)
                .OnComplete(() => IniciarParpadeo());
        }
        else
        {
            IniciarParpadeo();
        }
    }

    protected override void OnMenuClosed()
    {
        esperandoInput = false;
        DetenerEfectos();
    }

    private void Update()
    {
        if (esperandoInput && DetectarCualquierInput())
        {
            AbrirMenuPrincipal();
        }
    }

    private bool DetectarCualquierInput()
    {
        if (InputJugador.instance == null) return false;

        // Detectar cualquier input de UI
        if (InputJugador.instance.Confirmar ||
            InputJugador.instance.Cancelar ||
            InputJugador.instance.Click ||
            InputJugador.instance.Navegar != Vector2.zero)
        {
            return true;
        }

        // Detectar cualquier tecla del teclado
        if (Input.inputString.Length > 0)
        {
            return true;
        }

        // Detectar cualquier botón del gamepad
        if (Input.GetKeyDown(KeyCode.JoystickButton0) ||
            Input.GetKeyDown(KeyCode.JoystickButton1) ||
            Input.GetKeyDown(KeyCode.JoystickButton2) ||
            Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            return true;
        }

        return false;
    }

    private void IniciarParpadeo()
    {
        if (efectoParpadeoPtext && textoPressAnyKey != null)
        {
            tweenParpadeo = textoPressAnyKey.DOFade(0.3f, duracionParpadeo)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }

    private void DetenerEfectos()
    {
        tweenParpadeo?.Kill();
        tweenFadeIn?.Kill();
    }

    private void AbrirMenuPrincipal()
    {
        esperandoInput = false;
        DetenerEfectos();

        MenuManager.Instance?.OpenMenu(MenuManager.Instance.MenuPrincipal);
    }

    private void OnDestroy()
    {
        DetenerEfectos();
    }
}
