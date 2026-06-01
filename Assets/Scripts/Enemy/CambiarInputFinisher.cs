using UnityEngine;

public class CambiarInputFinisher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject CanvasControl;
    [SerializeField] private GameObject CanvasTeclado;

    private void OnEnable()
    {
        ActualizarContenedor();

        if (InputJugador.instance != null)
            InputJugador.instance.OnControlSchemeChanged += OnControlSchemeChanged;
    }
    private void OnControlSchemeChanged(string nuevoControl)
    {
        ActualizarContenedor();
    }

    private void ActualizarContenedor()
    {
        if (InputJugador.instance == null) return;

        bool usaGamepad = InputJugador.instance.UsaGamepad;
        CanvasControl.SetActive(usaGamepad);
        CanvasTeclado.SetActive(!usaGamepad);
    }
}
