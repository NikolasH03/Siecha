using UnityEngine;
using UnityEngine.UI;
public class MenuCombos : MenuBaseConNavegacion
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