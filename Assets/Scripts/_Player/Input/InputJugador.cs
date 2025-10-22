using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputJugador : MonoBehaviour
{

    public static InputJugador instance;

    private PlayerInput playerInput;

    // ========== GAMEPLAY INPUTS ==========
    public Vector2 moverse { get; private set; }
    public Vector2 mirar { get; private set; }

    public bool correr { get; private set; }
    public bool apuntar { get; private set; }
    public bool recargar { get; private set; }
    public bool atacarFuerte { get; private set; }
    public bool esquivar { get; private set; }
    public bool bloquear { get; private set; }

    public bool cambiarArmaMelee { get; private set; }
    public bool cambiarArmaDistancia { get; private set; }
    public bool cambiarProtagonista { get; private set; }

    public bool Interactuar { get; private set; }
    public bool AbrirMenuPausa { get; set; }

    // ========== ATAQUES CON INTERACCIONES ESPECIALES ==========

    private InputAction ataqueLigeroAction;
    private InputAction ataqueDistanciaAction;

    public bool AtaqueLigero { get; private set; }
    public bool holdStart { get; private set; }
    public bool holdSuccess { get; private set; }
    public bool holdFail { get; private set; }

    // ========== UI INPUTS ==========
    public Vector2 Navegar { get; private set; }
    public Vector2 Visualizar { get; private set; }
    public bool Confirmar { get; private set; }
    public bool Cancelar { get; private set; }
    public Vector2 Point { get; private set; }
    public bool Click { get; private set; }
    public bool CambiarTabDerecha { get; private set; }
    public bool CambiarTabIzquierda { get; private set; }

    private InputAction navegarAction;
    private InputAction pointAction;
    private InputAction visualizarAction;

    //Variables necesarias para el finisher

    private float tiempoUltimoLigero = -999f;
    private float tiempoUltimoFuerte = -999f;
    [SerializeField] private float ventanaFinisher = 0.25f;
    private float tiempoUltimoFinisher = -999f;
    [SerializeField] private float bufferFinisher = 0.15f;
    public bool FinisherInput => Time.time - tiempoUltimoFinisher <= bufferFinisher;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        playerInput = GetComponent<PlayerInput>();

        var meleeMap = playerInput.actions.FindActionMap("GameplayMelee", true);
        var distanciaMap = playerInput.actions.FindActionMap("GameplayDistancia", true);
        var uiMap = playerInput.actions.FindActionMap("UI", true);

        ataqueLigeroAction = meleeMap.FindAction("AtaqueLigero", true);
        ataqueDistanciaAction = distanciaMap.FindAction("Disparar", true);

        navegarAction = uiMap?.FindAction("Navegar", true);
        pointAction = uiMap?.FindAction("Point", true);
        visualizarAction = uiMap?.FindAction("Visualizar", true);
    }
    private void Update()
    {
        if (playerInput.currentActionMap.name == "UI")
        {
            if (navegarAction != null)
                Navegar = navegarAction.ReadValue<Vector2>();

            if (pointAction != null)
                Point = pointAction.ReadValue<Vector2>();

            if (visualizarAction != null)
                Visualizar = visualizarAction.ReadValue<Vector2>();
        }
    }
    public void OnMoverse(InputValue value) => moverse = value.Get<Vector2>();
    public void OnMirar(InputValue value) => mirar = value.Get<Vector2>();
    public void OnCorrer(InputValue value) => correr = value.isPressed;
    public void OnApuntar(InputValue value) => apuntar = value.isPressed;
    public void OnRecargar(InputValue value) => recargar = value.isPressed;
    public void OnAtaqueFuerte(InputValue value)
    {
        atacarFuerte = value.isPressed;

        if (value.isPressed)
        {
            tiempoUltimoFuerte = Time.time;

            if (Time.time - tiempoUltimoLigero <= ventanaFinisher)
            {
                tiempoUltimoFinisher = Time.time;
            }
        }
    }
    public void OnEsquivar(InputValue value) => esquivar = value.isPressed;
    public void OnBloquear(InputValue value) => bloquear = value.isPressed;

    public void OnCambiarArmaMelee(InputValue value) => cambiarArmaMelee = value.isPressed;
    public void OnCambiarArmaDistancia(InputValue value) => cambiarArmaDistancia = value.isPressed;

    public void OnCambiarProtagonista(InputValue value) => cambiarProtagonista = value.isPressed;
    public void OnInteractuar(InputValue value) => Interactuar = value.isPressed;
    public void OnAbrirMenuPausa(InputValue value) => AbrirMenuPausa = value.isPressed;

    //Inputs UI
    public void OnConfirmar(InputValue value) => Confirmar = value.isPressed;
    public void OnCancelar(InputValue value) => Cancelar = value.isPressed;
    public void OnClick(InputValue value) => Click = value.isPressed;
    public void OnCambiarTabDerecha(InputValue value) => CambiarTabDerecha = value.isPressed;
    public void OnCambiarTabIzquierda(InputValue value) => CambiarTabIzquierda = value.isPressed;

    private void LateUpdate()
    {
        recargar = false;
        atacarFuerte = false;
        esquivar = false;
        cambiarArmaMelee = false;
        cambiarArmaDistancia = false;
        cambiarProtagonista = false;
        Interactuar = false;
        AbrirMenuPausa = false;
        AtaqueLigero = false;
        holdStart = false;
        holdSuccess = false;
        holdFail = false;
        Confirmar = false;
        Cancelar = false;
        Click = false;
        CambiarTabDerecha = false;
        CambiarTabIzquierda = false;
    }

    public void CambiarInputDistancia()
    {
        playerInput.SwitchCurrentActionMap("GameplayDistancia");
    }
    public void CambiarInputMelee()
    {
        playerInput.SwitchCurrentActionMap("GameplayMelee");
    }
    public void CambiarInputUI()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }
    private string ultimoGameplayMap = "GameplayMelee";

    public void GuardarUltimoGameplayMap()
    {
        if (playerInput.currentActionMap.name.StartsWith("Gameplay"))
        {
            ultimoGameplayMap = playerInput.currentActionMap.name;
        }
    }

    public void VolverAGameplay()
    {
        playerInput.SwitchCurrentActionMap(ultimoGameplayMap);
    }

    private void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;

        ataqueLigeroAction.started += OnAtaqueStarted;
        ataqueLigeroAction.performed += OnAtaquePerformed;
        ataqueLigeroAction.canceled += OnAtaqueCanceled;

        ataqueDistanciaAction.started += OnAtaqueStarted;
        ataqueDistanciaAction.performed += OnAtaquePerformed;
        ataqueDistanciaAction.canceled += OnAtaqueCanceled;

    }

    private void OnDisable()
    {
        ataqueLigeroAction.started -= OnAtaqueStarted;
        ataqueLigeroAction.performed -= OnAtaquePerformed;
        ataqueLigeroAction.canceled -= OnAtaqueCanceled;

        ataqueDistanciaAction.started -= OnAtaqueStarted;
        ataqueDistanciaAction.performed -= OnAtaquePerformed;
        ataqueDistanciaAction.canceled -= OnAtaqueCanceled;
    }

    private void OnAtaqueStarted(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is HoldInteraction)
        {
            holdStart = true;
            //Debug.Log("comienza el hold para: "+playerInput.currentActionMap);
        }
    }

    private void OnAtaquePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is TapInteraction)
        {
            AtaqueLigero = true;

            tiempoUltimoLigero = Time.time;

            if (Time.time - tiempoUltimoFuerte <= ventanaFinisher)
            {
                tiempoUltimoFinisher = Time.time;
            }
        }
        else if (ctx.interaction is HoldInteraction)
        {
            holdSuccess = true;
            //Debug.Log("cargado exitoso para: " + playerInput.currentActionMap);
        }
    }

    private void OnAtaqueCanceled(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is HoldInteraction)
        {
            holdFail = true;
            //Debug.Log("cargado fallido para: " + playerInput.currentActionMap);
        }
    }
    private void OnControlsChanged(PlayerInput obj)
    {
        ResetGameplayInputs();
    }
    private void ResetGameplayInputs()
    {
        moverse = Vector2.zero;
        mirar = Vector2.zero;

        correr = false;
        apuntar = false;
        recargar = false;

        atacarFuerte = false;
        esquivar = false;
        bloquear = false;

        cambiarArmaMelee = false;
        cambiarArmaDistancia = false;
        cambiarProtagonista = false;

        Interactuar = false;
        AbrirMenuPausa = false;
        AtaqueLigero = false;

        holdStart = false;
        holdSuccess = false;
        holdFail = false;
    }


    public PlayerInput GetInputJugador()
    {
        return playerInput;
    }
}


