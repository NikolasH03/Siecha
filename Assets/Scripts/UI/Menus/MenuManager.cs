using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menús del Sistema")]
    [SerializeField] private MenuInicial menuInicial;
    [SerializeField] private MenuPrincipal menuPrincipal;
    [SerializeField] private MenuPausa menuPausa;
    [SerializeField] private MenuControles menuControles;
    [SerializeField] private MenuCreditos menuCreditos;
    [SerializeField] private MenuVolumen menuVolumen;
    [SerializeField] private MenuGraficos menuGraficos;
    [SerializeField] private MenuColeccionables menuColeccionables;
    [SerializeField] private MenuVisualizador3D menuVisualizador3D;

    [Header("Paneles de Gameplay")]
    [SerializeField] private MenuMuerteTisqa menuMuerteTisqa;  
    [SerializeField] private MenuMuertePaco menuMuertePaco;
    [SerializeField] private MenuTotem menuTotem;
    [SerializeField] private List<PanelTutorial> PanelesTutorial;

    [Header("Configuración de Escenas")]
    [SerializeField] private string[] escenasMenuPrincipal = { "Menu" };
    [SerializeField] private string[] escenasGameplay = { "Capitulo1-Introduccion" };


    private Stack<MenuBase> menuStack = new Stack<MenuBase>();
    private MenuBase currentMenu;

    public bool EstaEnGameplay => EsEscenaDeGameplay(SceneManager.GetActiveScene().name);
    public bool EstaEnMenuPrincipal => EsEscenaDeMenuPrincipal(SceneManager.GetActiveScene().name);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (EstaEnMenuPrincipal && menuInicial != null && !menuInicial.IsOpen)
        {
            OpenMenu(menuInicial);
        }
    }

    private void Update()
    {
        if (InputJugador.instance?.AbrirMenuPausa == true)
        {
            if (menuPausa != null && (menuPrincipal == null || !menuPrincipal.IsOpen))
            {
                OpenMenu(menuPausa);
                ControladorCambiarPersonaje.instance.OcultarTodosLosHUD();
            }
                

        }

        if (InputJugador.instance != null &&
            InputJugador.instance.GetInputJugador().currentActionMap.name == "UI" &&
            InputJugador.instance.Cancelar)
        {
            if (currentMenu != menuControles && currentMenu != menuVolumen && currentMenu != menuGraficos)
            {
                GoBack();
            }
            else
            {
                GoBackToPreviousCoreMenu();
            }
            
        }
    }
    // NUEVO: Verificar si hay paneles de gameplay activos
    private bool EsPanelDeGameplayActivo()
    {
        return (currentMenu == menuMuerteTisqa ||
                currentMenu == menuMuertePaco ||
                currentMenu == menuTotem);
    }

    // NUEVOS: Métodos para abrir paneles desde FSM y tótems
    public void MostrarPanelMuerteTisqa()
    {
        if (menuMuerteTisqa != null && !menuMuerteTisqa.IsOpen)
        {
            OpenMenu(menuMuerteTisqa);
            Debug.Log("Panel muerte tisqa abierto");
        }
    }

    public void MostrarPanelMuertePaco()
    {
        if (menuMuertePaco != null && !menuMuertePaco.IsOpen)
        {
            OpenMenu(menuMuertePaco);
            Debug.Log("Panel muerte paco abierto");
        }
    }
    public void MostrarPanelTotem()
    {
        if (menuTotem != null && !menuTotem.IsOpen)
        {
            OpenMenu(menuTotem);
            Debug.Log("Panel totem abierto");
        }
    }
    // ========== EVENTOS DE CAMBIO DE ESCENA ==========
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseAllMenus();

        if (EsEscenaDeMenuPrincipal(scene.name))
        {
            StartCoroutine(AbrirMenuInicialEnProximoFrame());
        }
        else if (EsEscenaDeGameplay(scene.name))
        {
            InputJugador.instance?.VolverAGameplay();
        }
    }
    private System.Collections.IEnumerator AbrirMenuInicialEnProximoFrame()
    {
        yield return null;
        if (menuInicial != null)
        {
            OpenMenu(menuInicial);
        }
    }

    private System.Collections.IEnumerator AbrirMenuPrincipalEnProximoFrame()
    {
        yield return null; // Esperar un frame para que todo se inicialice
        if (menuPrincipal != null)
        {
            OpenMenu(menuPrincipal);
        }
    }

    public void OpenMenu(MenuBase menu)
    {
        // Solo guardar gameplay map si estamos viniendo desde gameplay
        if (EstaEnGameplay)
        {
            InputJugador.instance?.GuardarUltimoGameplayMap();
        }

        if (currentMenu != null)
        {
            Debug.Log($"Cerrando menú actual: {currentMenu.name} y agregándolo al stack");
            menuStack.Push(currentMenu);
            currentMenu.CloseMenu();
        }

        currentMenu = menu;
        currentMenu.OpenMenu();

        // Cambiar a input de UI
        InputJugador.instance?.CambiarInputUI();
    }

    public void GoBack()
    {
        if (menuStack.Count > 0)
        {
            if (currentMenu != null)
                currentMenu.CloseMenu();

            currentMenu = menuStack.Pop();
            currentMenu.OpenMenu();

            // Mantener en UI porque hay más menús
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

            // Último menú
            currentMenu.CloseMenu();
            currentMenu = null;

            if (EstaEnGameplay)
            {
                // Volver a gameplay
                ControladorCambiarPersonaje.instance.ActivarHUDPausa();
                InputJugador.instance?.VolverAGameplay();
            }
            else if (EstaEnMenuPrincipal)
            {
                if (menuInicial != null)
                {
                    OpenMenu(menuInicial);
                }
            }
        }
    }

    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            menuStack.Pop().CloseMenu();
        }

        if (currentMenu != null)
        {
            currentMenu.CloseMenu();
            currentMenu = null;
        }

        // Decidir input según contexto
        if (EstaEnGameplay)
        {
            InputJugador.instance?.VolverAGameplay();
        }
        // En menú principal, mantener UI input
    }
    public void GoBackToPreviousCoreMenu()
    {
        // Cerrar el menú actual
        if (currentMenu != null)
            currentMenu.CloseMenu();

        // Vaciar todos los submenús del stack que sean parte de las opciones
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

        if (EstaEnGameplay && menuPausa != null)
        {
            OpenMenu(menuPausa);
        }
        else if (EstaEnMenuPrincipal && menuPrincipal != null)
        {
            OpenMenu(menuPrincipal);
        }
    }
    public void AbrirPanelTutorial(int IndexPanel)
    {
        if (PanelesTutorial[IndexPanel + 1] != null)
            OpenMenu(PanelesTutorial[IndexPanel + 1]);
        else
            Debug.LogWarning($"No se encontró el panel de tutorial: {PanelesTutorial[IndexPanel + 1]}");
    }


    public bool EsEscenaDeGameplay(string nombreEscena)
    {
        foreach (string escena in escenasGameplay)
        {
            if (nombreEscena.Equals(escena, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public bool EsEscenaDeMenuPrincipal(string nombreEscena)
    {
        foreach (string escena in escenasMenuPrincipal)
        {
            if (nombreEscena.Equals(escena, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }


    // Métodos de acceso rápido
    public MenuInicial MenuInicial => menuInicial;
    public MenuPrincipal MenuPrincipal => menuPrincipal;
    public MenuPausa MenuPausa => menuPausa;
    public MenuControles MenuControles => menuControles;
    public MenuCreditos MenuCreditos => menuCreditos;
    public MenuVolumen MenuVolumen => menuVolumen;
    public MenuMuerteTisqa MenuMuerteTisqa => menuMuerteTisqa;
    public MenuMuertePaco MenuMuertePaco => menuMuertePaco;
    public MenuTotem MenuTotem => menuTotem;
    public MenuGraficos MenuGraficos => menuGraficos;
    public MenuColeccionables MenuColeccionables => menuColeccionables;
    public MenuVisualizador3D MenuVisualizador3D => menuVisualizador3D;
}
