using UnityEngine;
using UnityEngine.UI;
public class MenuColeccionables : MenuBaseConNavegacion
{
    [SerializeField] private UIColeccionables uiColeccionables;
    [SerializeField] private Button botonVolver;

    protected override void OnMenuOpened()
    {
        if (uiColeccionables != null)
            uiColeccionables.RefrescarUI();
    }

    protected override void ConfigurarNavegacion()
    {
        if (botonVolver)
        {
            primerSeleccionable = botonVolver;
        }
    }

    public void VolverAtras()
    {
        MenuManager.Instance.GoBack();
    }
}