using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menus del Sistema")]
    [SerializeField] private MenuInicial       menuInicial;
    [SerializeField] private MenuPrincipal     menuPrincipal;
    [SerializeField] private MenuPausa         menuPausa;
    [SerializeField] private MenuControles     menuControles;
    [SerializeField] private MenuCreditos      menuCreditos;
    [SerializeField] private MenuVolumen       menuVolumen;
    [SerializeField] private MenuGraficos      menuGraficos;
    [SerializeField] private MenuColeccionables menuColeccionables;
    [SerializeField] private MenuVisualizador3D menuVisualizador3D;

    [Header("Paneles de Gameplay")]
    [SerializeField] private MenuMuerteTisqa   menuMuerteTisqa;
    [SerializeField] private MenuMuertePaco    menuMuertePaco;
    [SerializeField] private MenuTotem         menuTotem;
    [SerializeField] private List<PanelTutorial> PanelesTutorial;

    [Header("Configuracion de Escenas")]
    [SerializeField] private string[] escenasMenuPrincipal = { "Menu" };
    [SerializeField] private string[] escenasGameplay      = { "Capitulo1-Introduccion" };

    private Stack<MenuBase> menuStack    = new Stack<MenuBase>();
    private MenuBase        currentMenu;

    // FIX: flag centralizado para bloquear el menú de pausa durante cinemáticas.
    // CinematicaManager llama IniciarModoCinematica/TerminarModoCinematica.
    // Antes la lógica estaba en CinematicaJefe con una inyección de InputJugador.
    private bool enCinematica = false;

    public bool EstaEnGameplay      => EsEscenaDeGameplay(SceneManager.GetActiveScene().name);
    public bool EstaEnMenuPrincipal => EsEscenaDeMenuPrincipal(SceneManager.GetActiveScene().name);

    // ─── Singleton ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (EstaEnMenuPrincipal && menuInicial != null && !menuInicial.IsOpen)
            OpenMenu(menuInicial);
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        // No abrir menú de pausa durante cinemáticas
        if (!enCinematica && InputJugador.instance?.AbrirMenuPausa == true)
        {
            if (menuPausa != null && (menuPrincipal == null || !menuPrincipal.IsOpen))
            {
                // if (ControladorCambiarPersonaje.instance.PuedePausar)
                // {
                    OpenMenu(menuPausa);
                    ControladorCambiarPersonaje.instance.OcultarTodosLosHUD();
                // }
            }
        }

        if (InputJugador.instance != null &&
            InputJugador.instance.GetInputJugador().currentActionMap.name == "UI" &&
            InputJugador.instance.Cancelar &&
            !enCinematica) // tampoco cancelar durante cinemáticas
        {
            if (currentMenu != menuControles && currentMenu != menuVolumen && currentMenu != menuGraficos)
                GoBack();
            else
                GoBackToPreviousCoreMenu();
        }
    }

    // ─── API de Cinemáticas ───────────────────────────────────────────────────
    // Llamados por CinematicaManager. MenuManager gestiona el estado de input
    // porque ya es el responsable de toda la navegación de UI/Input en el juego.

    /// <summary>
    /// Bloquea el menú de pausa y cambia el input a UI para que el jugador
    /// no pueda interactuar durante la cinemática.
    /// </summary>
    public void IniciarModoCinematica()
    {
        enCinematica = true;
        InputJugador.instance?.GuardarUltimoGameplayMap();
        InputJugador.instance?.CambiarInputUI();
    }

    /// <summary>
    /// Restaura el estado de input anterior y desbloquea el menú de pausa.
    /// </summary>
    public void TerminarModoCinematica()
    {
        enCinematica = false;
        if (EstaEnGameplay)
            InputJugador.instance?.VolverAGameplay();
    }

    // ─── Paneles de Gameplay ──────────────────────────────────────────────────

    public void MostrarPanelMuerteTisqa()
    {
        if (menuMuerteTisqa != null && !menuMuerteTisqa.IsOpen)
            OpenMenu(menuMuerteTisqa);
    }

    public void MostrarPanelMuertePaco()
    {
        if (menuMuertePaco != null && !menuMuertePaco.IsOpen)
            OpenMenu(menuMuertePaco);
    }

    public void MostrarPanelTotem()
    {
        if (menuTotem != null && !menuTotem.IsOpen)
            OpenMenu(menuTotem);
    }

    public void AbrirPanelTutorial(int indexPanel)
    {
        if (indexPanel >= 0 && indexPanel < PanelesTutorial.Count)
            OpenMenu(PanelesTutorial[indexPanel]);
        else
            Debug.LogWarning($"[MenuManager] Indice de tutorial invalido: {indexPanel}");
    }

    // ─── Navegación ───────────────────────────────────────────────────────────

    public void OpenMenu(MenuBase menu)
    {
        if (EstaEnGameplay)
            InputJugador.instance?.GuardarUltimoGameplayMap();

        if (currentMenu != null)
        {
            menuStack.Push(currentMenu);
            currentMenu.CloseMenu();
        }

        currentMenu = menu;
        currentMenu.OpenMenu();
        InputJugador.instance?.CambiarInputUI();
    }

    public void GoBack()
    {
        if (menuStack.Count > 0)
        {
            currentMenu?.CloseMenu();
            currentMenu = menuStack.Pop();
            currentMenu.OpenMenu();
        }
        else if (currentMenu != null)
        {
            if (currentMenu == menuPrincipal && menuInicial != null)
            {
                currentMenu.CloseMenu();
                currentMenu = menuInicial;
                currentMenu.OpenMenu();
                return;
            }

            currentMenu.CloseMenu();
            currentMenu = null;

            if (EstaEnGameplay)
            {
                ControladorCambiarPersonaje.instance.ActivarHUDPausa();
                InputJugador.instance?.VolverAGameplay();
            }
            else if (EstaEnMenuPrincipal && menuInicial != null)
            {
                OpenMenu(menuInicial);
            }
        }
    }

    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
            menuStack.Pop().CloseMenu();

        currentMenu?.CloseMenu();
        currentMenu = null;

        if (EstaEnGameplay)
            InputJugador.instance?.VolverAGameplay();
    }

    public void GoBackToPreviousCoreMenu()
    {
        currentMenu?.CloseMenu();

        while (menuStack.Count > 0)
        {
            var menu = menuStack.Pop();
            if (menu != menuControles && menu != menuVolumen && menu != menuGraficos)
            {
                currentMenu = menu;
                currentMenu.OpenMenu();
                return;
            }
        }

        if (EstaEnGameplay && menuPausa != null)         OpenMenu(menuPausa);
        else if (EstaEnMenuPrincipal && menuPrincipal != null) OpenMenu(menuPrincipal);
    }

    // ─── Cambio de escena ─────────────────────────────────────────────────────

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cambiar de escena siempre salimos del modo cinemática
        enCinematica = false;

        CloseAllMenus();

        if (EsEscenaDeMenuPrincipal(scene.name))
        {
            StartCoroutine(AbrirMenuInicialEnProximoFrame());
            GameDataManager.Instance.ReiniciarDatosJugador();
        }
            
        else if (EsEscenaDeGameplay(scene.name))
            InputJugador.instance?.VolverAGameplay();
    }

    private IEnumerator AbrirMenuInicialEnProximoFrame()
    {
        yield return null;
        if (menuInicial != null) OpenMenu(menuInicial);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    public bool EsEscenaDeGameplay(string nombre)
    {
        foreach (var e in escenasGameplay)
            if (nombre.Equals(e, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public bool EsEscenaDeMenuPrincipal(string nombre)
    {
        foreach (var e in escenasMenuPrincipal)
            if (nombre.Equals(e, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public bool EstaEnPausa() => currentMenu != null && currentMenu.Pause;

    // ─── Acceso rápido ────────────────────────────────────────────────────────

    public MenuInicial        MenuInicial        => menuInicial;
    public MenuPrincipal      MenuPrincipal      => menuPrincipal;
    public MenuPausa          MenuPausa          => menuPausa;
    public MenuControles      MenuControles      => menuControles;
    public MenuCreditos       MenuCreditos       => menuCreditos;
    public MenuVolumen        MenuVolumen        => menuVolumen;
    public MenuMuerteTisqa    MenuMuerteTisqa    => menuMuerteTisqa;
    public MenuMuertePaco     MenuMuertePaco     => menuMuertePaco;
    public MenuTotem          MenuTotem          => menuTotem;
    public MenuGraficos       MenuGraficos       => menuGraficos;
    public MenuColeccionables MenuColeccionables => menuColeccionables;
    public MenuVisualizador3D MenuVisualizador3D => menuVisualizador3D;
}