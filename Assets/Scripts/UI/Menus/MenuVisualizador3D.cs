using UnityEngine;
using UnityEngine.UI;

public class MenuVisualizador3D : MenuBaseConNavegacion
{
    [SerializeField] private UIVisualizador3D visualizador;
    [SerializeField] private Button botonVolver;

    protected override void ConfigurarNavegacion()
    {
        primerSeleccionable = botonVolver;
    }

    public void OnVolver()
    {
        visualizador.Cerrar();
        MenuManager.Instance.GoBack();
    }

    private void Update()
    {
        Vector2 mirar = InputJugador.instance.Visualizar;
        if (mirar.sqrMagnitude > 0.01f)
        {
            visualizador.RotarModelo(mirar);
        }
    }
}
