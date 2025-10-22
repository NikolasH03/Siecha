using UnityEngine;
using UnityEngine.UI;

public class PanelTutorial : MenuBaseConNavegacion
{
    [SerializeField] private Button botonCerrar;

    protected override void ConfigurarNavegacion()
    {
        if (botonCerrar)
            primerSeleccionable = botonCerrar;
    }

    public void VolverAtras()
    {
        MenuManager.Instance.GoBack();
    }
}