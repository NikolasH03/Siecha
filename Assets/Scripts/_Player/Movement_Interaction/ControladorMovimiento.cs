using UnityEngine.InputSystem;
using UnityEngine;

public class ControladorMovimiento : MonoBehaviour
{

    [SerializeField] float VelocidadCaminando = 200f;
    [SerializeField] float VelocidadCorriendo = 600f;
    [Range(0.0f, 0.3f)]
    [SerializeField] float RotationSmoothTime;
    [SerializeField] float SpeedChangeRate = 50f;

    [SerializeField] float sensibilidad = 1f;

    private Rigidbody rb;
    private bool canMove = true;

    [SerializeField] float _speed;
    [SerializeField] float _targetRotation = 0.0f;
    [SerializeField] float _rotationVelocity;
    [SerializeField] float _verticalVelocity;
    [SerializeField] GameObject _mainCamera;

    //coordenadas para animaciones de movimiento
    [SerializeField] float x, y;
    private Animator anim;

    //rotacion de la camara

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float BottomClamp = -30f;
    private const float TopClamp = 70f;
    private bool LockCameraPosition = false;
    private const float _threshold = 0.01f;
    private float CameraAngleOverride = 0.0f;
    [SerializeField] GameObject CinemachineCameraTarget;
    private bool rotarAlMoverse = true;

    //referencias a otro codigos
    private ControladorCombate controladorCombate;
    private ControladorApuntado controladorApuntado;
    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        rb = GetComponent<Rigidbody>();
        controladorCombate = GetComponent<ControladorCombate>();
        controladorApuntado = GetComponent<ControladorApuntado>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }

        else
        {

            if (!controladorCombate.getAtacando() && !controladorCombate.getBloqueando() && !anim.GetBool("dashing"))
            {
                ValoresAnimacionMovimiento();


                if (x == 0f && y == 0f)
                {
                    anim.SetBool("running", false);
                }
                else
                {
                    mover();

                }
            }

        }
    }

    private void LateUpdate()
    {
        RotacionCamara();
    }
    public void ValoresAnimacionMovimiento()
    {
        x = InputJugador.instance.moverse.x;
        y = InputJugador.instance.moverse.y;


        anim.SetFloat("Velx", x);
        anim.SetFloat("Vely", y);

    }

    public float CheckEstaCorriendo()
    {
        if (InputJugador.instance.correr && !controladorCombate.getAtacando() && canMove && !controladorApuntado.GetEstaApuntando())
        {
            anim.SetBool("running", true);
            return VelocidadCorriendo;
        }
        else if (!InputJugador.instance.correr || controladorCombate.getAtacando() || !canMove || controladorCombate.getBloqueando() || anim.GetBool("dashing") || controladorApuntado.GetEstaApuntando())
        {
            anim.SetBool("running", false);
            return VelocidadCaminando;
        }
        return VelocidadCaminando;
    }

    private void mover()
    {

        float targetSpeed = CheckEstaCorriendo();

        if (InputJugador.instance.moverse == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).magnitude;


        float speedOffset = 0.5f;


        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {

            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);


            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }



        Vector3 inputDirection = new Vector3(InputJugador.instance.moverse.x, 0.0f, InputJugador.instance.moverse.y).normalized;


        if (InputJugador.instance.moverse != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);


            if (rotarAlMoverse)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;


        rb.MovePosition(transform.position +
            (targetDirection.normalized * _speed * Time.deltaTime) +
            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);


    }


    private void RotacionCamara()
    {
        if (InputJugador.instance.mirar.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = 1.0f;

            _cinemachineTargetYaw += InputJugador.instance.mirar.x * deltaTimeMultiplier * sensibilidad;
            _cinemachineTargetPitch += InputJugador.instance.mirar.y * deltaTimeMultiplier * sensibilidad;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);

    }

    //setters y getters

    public void SetSensibilidad(float nuevaSensibilidad)
    {
        sensibilidad = nuevaSensibilidad;
    }

    public void SetRotacionAlMoverse(bool nuevoRotacionAlMoverse)
    {
        rotarAlMoverse = nuevoRotacionAlMoverse;
    }
    public bool getCanMove()
    {
        return canMove;
    }
    public void setCanMove(bool mover)
    {
        canMove = mover;
    }
}

