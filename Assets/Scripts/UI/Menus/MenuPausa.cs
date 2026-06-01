using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MenuBaseConNavegacion
{
    [Header("Botones del Menú Pausa")]
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonColeccionables;
    [SerializeField] private Button botonMenuPrincipal;
    [SerializeField] private Button botonSalir;

    private void Awake()
    {
        pauseGame = true;
    }

    protected override void ConfigurarNavegacion()
    {
        if (botonReanudar && botonOpciones && botonColeccionables && botonMenuPrincipal && botonSalir)
        {
            ConfigurarNavegacionBoton(botonReanudar,
                izquierda: botonSalir, derecha: botonOpciones);

            ConfigurarNavegacionBoton(botonOpciones,
                izquierda: botonReanudar, derecha: botonColeccionables);

            ConfigurarNavegacionBoton(botonColeccionables,
                izquierda: botonOpciones, derecha: botonMenuPrincipal);

            ConfigurarNavegacionBoton(botonMenuPrincipal,
                izquierda: botonColeccionables, derecha: botonSalir);

            ConfigurarNavegacionBoton(botonSalir,
                izquierda: botonMenuPrincipal, derecha: botonReanudar);


            primerSeleccionable = botonReanudar;
        }
    }
    //public void Start()
    //{
    //    AudioManager.Instance.PlaySFX(AudioManager.Instance.efecto_AbrirMenuPausa, this.transform.position);
    //}

    public void Reanudar()
    {
        CloseMenu();
        InputJugador.instance?.VolverAGameplay();
        ControladorCambiarPersonaje.instance.ActivarHUDPausa();
    }

    public void AbrirOpciones()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuControles);
    }

    public void AbrirColeccionables()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuColeccionables);
    }

    public void IrArbolHabilidades(string nivel)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nivel);
    }

    public void VolverMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        GameFlowManager.Instance.ReiniciarFlujoDeJuego();
    }
    public void SalirJuego()
    {
        Application.Quit();
    }
}