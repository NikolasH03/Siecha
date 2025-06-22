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
     private Animator animator;
    [SerializeField] private GameObject crosshair;
    [SerializeField] bool estaApuntando = false;

    //referencias a otros codigos
    private ControladorMovimiento controladorMovimiento;

    public void Start()
    {
        crosshair.SetActive(false);
    }
    private void Update()
    {
        controladorMovimiento = GetComponent<ControladorMovimiento>(); 
        animator = GetComponent<Animator>();

    }

    public Vector3 ObtenerPosicionObjetivo()
    {
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

        return posicionMouse;
    }

    public void EstaApuntando(Vector3 posicionMouse)
    {

        estaApuntando = true;
        camaraApuntado.gameObject.SetActive(true);
        crosshair.SetActive(true);
        controladorMovimiento.SetSensibilidad(sensibilidadApuntado);
        controladorMovimiento.SetRotacionAlMoverse(false);
        
        Vector3 TargetApuntado = posicionMouse;
        TargetApuntado.y = transform.position.y;
        Vector3 direccionApuntado = (TargetApuntado - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, direccionApuntado, Time.deltaTime * 20f);
    }

    public void NoEstaApuntando()
    {
        estaApuntando = false;
        camaraApuntado.gameObject.SetActive(false);
        controladorMovimiento.SetSensibilidad(sensibilidadNormal);
        controladorMovimiento.SetRotacionAlMoverse(true);
        crosshair.SetActive(false);
    }

    public void InstanciarBala(Vector3 posicionMouse)
    {
        Vector3 aimDir = (posicionMouse - spawnPosicionProyectil.position).normalized;
        Instantiate(prefabProyectil, spawnPosicionProyectil.position, Quaternion.LookRotation(aimDir, Vector3.up)); 
    }

    public void TransicionarLayerPeso(int layerIndex, float pesoObjetivo, float duracion)
    {
        StartCoroutine(TransicionLayerCoroutine(layerIndex, pesoObjetivo, duracion));
    }

    private IEnumerator TransicionLayerCoroutine(int layerIndex, float pesoObjetivo, float duracion)
    {
        float pesoInicial = animator.GetLayerWeight(layerIndex);
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float peso = Mathf.Lerp(pesoInicial, pesoObjetivo, tiempoTranscurrido / duracion);
            animator.SetLayerWeight(layerIndex, peso);
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, pesoObjetivo); 
    }

    public bool GetEstaApuntando()
    {
        return estaApuntando;
    }
    
}
