using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuColeccionables : MenuBaseConNavegacion
{
    [SerializeField] private UIColeccionables uiColeccionables;
    [SerializeField] private Button botonVolver;

    private List<Button> botonesColeccionables = new List<Button>();

    private void Awake()
    {

        // Suscribimos al evento antes de refrescar
        uiColeccionables.OnColeccionablesInstanciados += ConfigurarNavegacionDinamica;
        uiColeccionables.RefrescarUI();
    }

    private void OnDisable()
    {
        uiColeccionables.OnColeccionablesInstanciados -= ConfigurarNavegacionDinamica;
    }

    protected override void ConfigurarNavegacion() { } 

    private void ConfigurarNavegacionDinamica(List<Button> botones)
    {
        botonesColeccionables = botones;

        //Configurar navegación entre botones
        for (int i = 0; i < botonesColeccionables.Count; i++)
        {
            Button actual = botonesColeccionables[i];
            Button arriba = (i > 0) ? botonesColeccionables[i - 1] : botonVolver;
            Button abajo = (i < botonesColeccionables.Count - 1) ? botonesColeccionables[i + 1] : botonVolver;

            ConfigurarNavegacionBoton(actual, arriba, abajo);
        }

        //Configurar navegación del botón "Volver"
        if (botonVolver != null && botonesColeccionables.Count > 0)
        {
            Button ultimo = botonesColeccionables[botonesColeccionables.Count - 1];
            ConfigurarNavegacionBoton(botonVolver, arriba: ultimo, abajo: botonesColeccionables[0]);
        }

        //Definir cuál será el primer seleccionable
        primerSeleccionable = botonesColeccionables.Count > 0 ? botonesColeccionables[0] : botonVolver;

        //Forzar la selección ahora mismo (porque el menú ya está abierto)
        if (eventSystem != null && primerSeleccionable != null)
            eventSystem.SetSelectedGameObject(primerSeleccionable.gameObject);
    }

    public void OnVolver()
    {
        MenuManager.Instance.GoBack();
    }
}
