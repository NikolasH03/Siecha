using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuControles : MenuBaseConNavegacion
{
    [Header("UI de Controles")]
    [SerializeField] private GameObject layoutTeclado;
    [SerializeField] private GameObject layoutMando;

    [Header("UI de Combos")]
    [SerializeField] private GameObject listaCombosTeclado;
    [SerializeField] private GameObject listaCombosMando;

    [Header("Botón Volver")]
    [SerializeField] private Button botonVolver;

    [Header("Botones BarraOpciones")]
    [SerializeField] private Button botonControles;
    [SerializeField] private Button botonVolumen;
    [SerializeField] private Button botonGraficas;

    private ScrollRect scrollCombos;
    private RectTransform contentCombos;
    private Selectable[] combosSelectables;
    private string controlSchemeActual = "";
    private GameObject ultimoSeleccionado;

    private void Awake()
    {
        string controlScheme = InputJugador.instance.GetInputJugador().currentControlScheme;
        ActualizarLayout(controlScheme);
    }
    void Update()
    {
        string controlScheme = InputJugador.instance.GetInputJugador().currentControlScheme;

        if (controlScheme != controlSchemeActual)
        {
            controlSchemeActual = controlScheme;
            ActualizarLayout(controlScheme);
        }
    }


    private void ActualizarLayout(string controlScheme)
    {
        if (controlScheme == "Keyboard")
        {
            layoutTeclado.SetActive(true);
            listaCombosTeclado.SetActive(true);

            layoutMando.SetActive(false);
            listaCombosMando.SetActive(false);

            scrollCombos = listaCombosTeclado.GetComponentInChildren<ScrollRect>(true);
            contentCombos = scrollCombos?.content;
        }
        else if (controlScheme == "Console")
        {
            layoutTeclado.SetActive(false);
            listaCombosTeclado.SetActive(false);

            layoutMando.SetActive(true);
            listaCombosMando.SetActive(true);

            scrollCombos = listaCombosMando.GetComponentInChildren<ScrollRect>(true);
            contentCombos = scrollCombos?.content;
        }
    }

    protected override void ConfigurarNavegacion()
    {
        if (botonControles && botonVolumen && botonGraficas && botonVolver)
        {
            ConfigurarNavegacionBoton(botonControles,
               arriba: botonVolver, abajo: null, derecha: botonVolumen, izquierda: botonGraficas);
            ConfigurarNavegacionBoton(botonVolumen,
                arriba: botonVolver, abajo: null, derecha: botonGraficas, izquierda: botonControles);
            ConfigurarNavegacionBoton(botonGraficas,
                arriba: botonVolver, abajo: null, derecha: botonControles, izquierda: botonVolumen);
            ConfigurarNavegacionBoton(botonVolver,
                arriba: null, abajo: null, derecha: null, izquierda: null);

            primerSeleccionable = botonControles;
        }

        // --- Configurar combos dinámicamente ---
        if (contentCombos == null) return;

        combosSelectables = contentCombos.GetComponentsInChildren<Selectable>(includeInactive: false);
        if (combosSelectables.Length == 0) return;

        // Conectar la barra superior con los combos
        ConfigurarNavegacionBoton(botonControles,
            arriba: botonVolver,
            abajo: combosSelectables[0],
            derecha: botonVolumen,
            izquierda: botonGraficas);

        // Combos intermedios
        for (int i = 0; i < combosSelectables.Length; i++)
        {
            Selectable arriba = (i == 0) ? botonControles : combosSelectables[i - 1];
            Selectable abajo = (i == combosSelectables.Length - 1) ? botonVolver : combosSelectables[i + 1];
            Selectable derecha = botonVolver; 
            Selectable izquierda = botonVolver;

            ConfigurarNavegacionBoton(combosSelectables[i], arriba, abajo, derecha, izquierda);
        }

        // Conectar el botón Volver
        ConfigurarNavegacionBoton(botonVolver,
            arriba: combosSelectables[combosSelectables.Length - 1],
            abajo: botonControles,
            derecha: combosSelectables[0],
            izquierda: combosSelectables[0]);
    }

    private void LateUpdate()
    {
        // Detecta cambio de selección
        var actual = EventSystem.current.currentSelectedGameObject;
        if (actual != ultimoSeleccionado)
        {
            ultimoSeleccionado = actual;
            if (actual != null && scrollCombos != null)
                ScrollHaciaElemento(actual);
        }
    }

    private void ScrollHaciaElemento(GameObject seleccionado)
    {
        // Verifica que el seleccionado esté dentro del contenido del ScrollRect
        if (!seleccionado.transform.IsChildOf(contentCombos.transform))
            return;

        // Calcula la posición local normalizada
        RectTransform elemento = seleccionado.GetComponent<RectTransform>();
        RectTransform viewport = scrollCombos.viewport;

        Vector3[] viewCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];
        viewport.GetWorldCorners(viewCorners);
        elemento.GetWorldCorners(itemCorners);

        // Si el elemento está fuera del área visible, ajusta el scroll
        if (itemCorners[0].y < viewCorners[0].y)
            MoverScroll(1); // bajar
        else if (itemCorners[1].y > viewCorners[1].y)
            MoverScroll(-1); // subir
    }

    private void MoverScroll(float direccion)
    {
        float velocidad = 0.15f;
        float nuevo = Mathf.Clamp01(scrollCombos.verticalNormalizedPosition + direccion * velocidad);
        scrollCombos.verticalNormalizedPosition = nuevo;
    }

    // --- Métodos de navegación ---
    public void AbrirVolumen() => MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuVolumen);
    public void AbrirGraficos() => MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuGraficos);
    public void VolverSinGuardar() => MenuManager.Instance.GoBackToPreviousCoreMenu();
}
