using UnityEngine;
using UnityEngine.InputSystem;

public class InputJugador : MonoBehaviour
{

    public static InputJugador instance;

    private PlayerInput playerInput;
    public Vector2 moverse { get; private set; }
    public Vector2 mirar { get; private set; }

    public bool correr { get; private set; }
    public bool apuntar { get; private set; }
    public bool disparar { get; private set; }
    public bool recargar { get; private set; }

    public bool atacarLigero { get; private set; }
    public bool atacarFuerte { get; private set; }
    public bool esquivar { get; private set; }
    public bool bloquear { get; private set; }

    public bool cambiarArmaMelee { get; private set; }
    public bool cambiarArmaDistancia { get; private set; }
    public bool cambiarProtagonista { get; private set; }


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

    }

    private void Start()
    {
       playerInput = GetComponent<PlayerInput>();

    }

    public void OnMoverse(InputValue value) => moverse = value.Get<Vector2>();
    public void OnMirar(InputValue value) => mirar = value.Get<Vector2>();
    public void OnCorrer(InputValue value) => correr = value.isPressed;
    public void OnApuntar(InputValue value) => apuntar = value.isPressed;
    public void OnDisparar(InputValue value) => disparar = value.isPressed;
    public void OnRecargar(InputValue value) => recargar = value.isPressed;

    public void OnAtaqueLigero(InputValue value) => atacarLigero = value.isPressed;
    public void OnAtaqueFuerte(InputValue value) => atacarFuerte = value.isPressed;
    public void OnEsquivar(InputValue value) => esquivar = value.isPressed;
    public void OnBloquear(InputValue value) => bloquear = value.isPressed;

    public void OnCambiarArmaMelee(InputValue value) => cambiarArmaMelee = value.isPressed;
    public void OnCambiarArmaDistancia(InputValue value) => cambiarArmaDistancia = value.isPressed;

    public void OnCambiarProtagonista(InputValue value) => cambiarProtagonista = value.isPressed;

    private void LateUpdate()
    {
        disparar = false;
        recargar = false;
        atacarLigero = false;
        atacarFuerte = false;
        esquivar = false;
        cambiarArmaMelee = false;
        cambiarArmaDistancia = false;
        cambiarProtagonista = false;
    }

    public void CambiarInputDistancia()
    {
        playerInput.SwitchCurrentActionMap("GameplayDistancia");
    }
    public void CambiarInputMelee()
    {
        playerInput.SwitchCurrentActionMap("GameplayMelee");
    }

}


