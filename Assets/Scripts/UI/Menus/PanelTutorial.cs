using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelTutorial : MenuBaseConNavegacion
{
    [Header("Botones y contenedores")]
    [SerializeField] private Button botonCerrar;
    [SerializeField] private GameObject contenedorTeclado;
    [SerializeField] private GameObject contenedorGamepad;

    protected override void ConfigurarNavegacion()
    {
        if (botonCerrar)
            primerSeleccionable = botonCerrar;
    }

    private void OnEnable()
    {
        eventSystem = EventSystem.current;
        ConfigurarNavegacion();

        ActualizarContenedor();

        if (InputJugador.instance != null)
            InputJugador.instance.OnControlSchemeChanged += OnControlSchemeChanged;
    }

    private void OnDisable()
    {
        if (InputJugador.instance != null)
            InputJugador.instance.OnControlSchemeChanged -= OnControlSchemeChanged;
    }

    private void OnControlSchemeChanged(string nuevoControl)
    {
        ActualizarContenedor();
    }

    private void ActualizarContenedor()
    {
        if (InputJugador.instance == null) return;

        bool usaGamepad = InputJugador.instance.UsaGamepad;
        contenedorTeclado.SetActive(!usaGamepad);
        contenedorGamepad.SetActive(usaGamepad);
    }

    public void VolverAtras()
    {
        MenuManager.Instance.GoBack();
    }
}
