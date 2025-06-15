using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
public class ControladorApuntado : MonoBehaviour
{
   [SerializeField] private CinemachineVirtualCamera camaraApuntado;
    [SerializeField] private float sensibilidadNormal;
    [SerializeField] private float sensibilidadApuntado;
    [SerializeField] private LayerMask apuntadoColliderLayerMask = new LayerMask();
    [SerializeField] private Transform prefabProyectil;
    [SerializeField] private Transform spawnPosicionProyectil;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private bool estaEnAnimacionDisparo = false;
    [SerializeField] bool estaApuntando = false;
    //[SerializeField] bool yaRecargo = false;

    //referencias a otros codigos
    private ControladorCambioArmas controladorCambioArmas;
    private ControladorMovimiento controladorMovimiento;
    private ControladorCombate controladorCombate;

    public void Start()
    {
        crosshair.SetActive(false);
    }
    private void Update()
    {
        controladorCambioArmas = GetComponent<ControladorCambioArmas>();
        controladorMovimiento = GetComponent<ControladorMovimiento>();
        controladorCombate = GetComponent<ControladorCombate>();    
        animator = GetComponent<Animator>();
        int numeroArma = controladorCambioArmas.getterArma();

        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(centroPantalla);
        Vector3 posicionMouse = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, apuntadoColliderLayerMask))
        {
            posicionMouse = raycastHit.point;
        }
        else
        {
            centroPantalla = ray.GetPoint(10);
            posicionMouse = ray.GetPoint(10);
        }


        if (numeroArma == 2)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            RealizaAccionMientrasApunta();

            if (InputJugador.instance.apuntar)
            {
                EstaApuntando(posicionMouse);
               
                if (InputJugador.instance.disparar)
                {
                    Debug.Log("detecto el click");
                    if (!estaEnAnimacionDisparo)
                    {
                        Debug.Log("va a instanciar la bala");
                        disparar(posicionMouse);
                    }

                }

            }
            else
            {
                noEstaApuntando();
            }
        }
        else if (numeroArma == 1)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }
    }

    public void EstaApuntando(Vector3 posicionMouse)
    {

        estaApuntando = true;
        animator.SetBool("Apuntando",true);
        camaraApuntado.gameObject.SetActive(true);
        crosshair.SetActive(true);
        controladorMovimiento.SetSensibilidad(sensibilidadApuntado);
        controladorMovimiento.SetRotacionAlMoverse(false);
        
        Vector3 TargetApuntado = posicionMouse;
        TargetApuntado.y = transform.position.y;
        Vector3 direccionApuntado = (TargetApuntado - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, direccionApuntado, Time.deltaTime * 20f);
    }

    public void noEstaApuntando()
    {
        estaApuntando = false;
        animator.SetBool("Apuntando", false);
        camaraApuntado.gameObject.SetActive(false);
        controladorMovimiento.SetSensibilidad(sensibilidadNormal);
        controladorMovimiento.SetRotacionAlMoverse(true);
        crosshair.SetActive(false);
    }

    public void disparar(Vector3 posicionMouse)
    {
        Vector3 aimDir = (posicionMouse - spawnPosicionProyectil.position).normalized;

        animator.Play("fire");
        controladorCombate.AnimationEvent_ReproducirVFX(4, 2);
        controladorCombate.AnimationEvent_ReproducirSonido(4, 2);
        animator.SetBool("Disparo",true);        
        Instantiate(prefabProyectil, spawnPosicionProyectil.position, Quaternion.LookRotation(aimDir, Vector3.up)); 
        estaEnAnimacionDisparo = true;


    }
    public void recargar()
    {
        animator.SetBool("Disparo", false);
        estaEnAnimacionDisparo = false;
        InputJugador.instance.disparar = false;
    }

    public void RealizaAccionMientrasApunta()
    {
        if (controladorMovimiento.getAnim().GetBool("RecibeDaño") || controladorMovimiento.getAnim().GetBool("Muere") || controladorCombate.getDashing() || controladorCombate.getBloqueando())
        {
            InputJugador.instance.apuntar = false;
            estaApuntando = false;
            camaraApuntado.gameObject.SetActive(false);
            controladorMovimiento.SetSensibilidad(sensibilidadNormal);
            controladorMovimiento.SetRotacionAlMoverse(true);
            crosshair.SetActive(false);
            StartCoroutine(AjustarLayer(1, 0f, 0.2f));



        }
        else if (!controladorMovimiento.getAnim().GetBool("RecibeDaño") && !controladorMovimiento.getAnim().GetBool("Muere") && !controladorCombate.getDashing() && !controladorCombate.getBloqueando())
        {
            VolverLayerNormal();
        }
   
    }

    IEnumerator AjustarLayer(int layerIndex, float targetWeight, float duration)
    {
        float startWeight = animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, targetWeight, elapsedTime / duration);
            animator.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, targetWeight);


    }

    public void VolverLayerNormal()
    {
        StartCoroutine(AjustarLayer(1, 1f, 0.2f));
    }

    public bool GetEstaApuntando()
    {
        return estaApuntando;
    }
    
}
