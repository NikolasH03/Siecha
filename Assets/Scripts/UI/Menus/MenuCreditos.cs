using UnityEngine;
using UnityEngine.UI;

public class MenuCreditos : MenuBaseConNavegacion
{
    [SerializeField] private Button botonVolver;

    protected override void ConfigurarNavegacion()
    {
        if (botonVolver)
            primerSeleccionable = botonVolver;
    }

    public void VolverAtras()
    {
        MenuManager.Instance.GoBack();
    }
}