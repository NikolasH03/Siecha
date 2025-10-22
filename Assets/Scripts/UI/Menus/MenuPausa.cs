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
                arriba: botonSalir, abajo: botonOpciones);

            ConfigurarNavegacionBoton(botonOpciones,
                arriba: botonReanudar, abajo: botonColeccionables);

            ConfigurarNavegacionBoton(botonColeccionables,
                arriba: botonOpciones, abajo: botonMenuPrincipal);

            ConfigurarNavegacionBoton(botonMenuPrincipal,
                arriba: botonColeccionables, abajo: botonSalir);

            ConfigurarNavegacionBoton(botonSalir,
                arriba: botonMenuPrincipal, abajo: botonReanudar);


            primerSeleccionable = botonReanudar;
        }
    }

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
    }
    public void SalirJuego()
    {
        Application.Quit();
    }
}