using UnityEngine;
using UnityEngine.InputSystem;

public class InputJugador : MonoBehaviour
{

    public Vector2 moverse;

    public Vector2 mirar;

    public bool correr;

    public bool apuntar;

    public bool disparar;


    public static InputJugador instance;

    ControladorCambioArmas controladorCambioArmas;
    ControladorApuntado controladorApuntado;

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
        var playerInput = GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("GameplayMelee");

    }
    private void Update()
    {
        controladorCambioArmas = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCambioArmas>();
        controladorApuntado = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorApuntado>();
    }

    public void OnMoverse(InputValue value)
    {

        moverse = value.Get<Vector2>();
    }

    public void OnMirar(InputValue value)
    {
        mirar = value.Get<Vector2>();
    }
    public void OnCorrer(InputValue value)
    {
        correr = value.isPressed;
    }
    public void OnApuntar(InputValue value)
    {
        apuntar = value.isPressed;
    }
    public void OnDisparar(InputValue value)
    {
        int tipoArma = controladorCambioArmas.getterArma();

        if (tipoArma == 2)
        {
            if (controladorApuntado.GetEstaApuntando())
            {
                disparar = value.isPressed;
            }
        }
        
        
    }
}


