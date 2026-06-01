using UnityEngine;
using UnityEngine.UI;
public class MenuPrincipal : MenuBaseConNavegacion
{
    [Header("Botones del Men� Principal")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonCreditos;
    [SerializeField] private Button botonSalir;

    protected override void ConfigurarNavegacion()
    {
        if (botonJugar && botonOpciones && botonCreditos && botonSalir)
        {

            ConfigurarNavegacionBoton(botonJugar,
                arriba: botonSalir, abajo: botonOpciones);

            ConfigurarNavegacionBoton(botonOpciones,
                arriba: botonJugar, abajo: botonCreditos);

            ConfigurarNavegacionBoton(botonCreditos,
                arriba: botonOpciones, abajo: botonSalir);

            ConfigurarNavegacionBoton(botonSalir,
                arriba: botonCreditos, abajo: botonJugar);

            primerSeleccionable = botonJugar;
        }
    }

    public void Jugar()
    {
        Time.timeScale = 1f;
        MenuManager.Instance.CloseAllMenus();
        SceneLoader.Instance.LoadScene(1);
        GameFlowManager.Instance.StartGameplayFlow();
    }

    public void AbrirOpciones()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuControles);
    }

    public void AbrirCreditos()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuCreditos);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}