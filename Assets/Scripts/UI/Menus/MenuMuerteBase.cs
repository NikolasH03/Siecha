using UnityEngine;
using UnityEngine.UI;

public abstract class MenuMuerteBase : MenuBaseConNavegacion
{
    [Header("UI Panel Muerte")]
    [SerializeField] protected Button botonContinuar;

    private void Awake()
    {
        pauseGame = true; 
    }
    protected override void ConfigurarNavegacion()
    {
        if (botonContinuar)
        {
            primerSeleccionable = botonContinuar;
        }
    }
    protected override void OnMenuOpened()
    {
        InputJugador.instance?.CambiarInputUI();
    }

    protected override void OnMenuClosed()
    {
    }

    public abstract void Continuar();
}