using System.Collections;
using UnityEngine;
using UnityEngine.UI;  

public class Arco : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject aimCamera;
    public Image aimReticle; 
    public GameObject bowPosition;
    public GameObject arrowPrefab;
    public float fuerzaMinima = 10f;
    public float fuerzaMaxima = 50f;
    public float tiempoDeCarga = 2f;
    private float tiempoDeApuntar;
    private GameObject flechaActual;
    public bool isAiming;

    [SerializeField] Player player;
    private void Start()
    {
        aimReticle.enabled = false;
        aimCamera.SetActive(false);
    }
    void Update()
    {
        // Manejando el valor de apuntado (botón derecho del ratón)
        if (Input.GetMouseButtonDown(1))
        {
            tiempoDeApuntar = 0f;
            mainCamera.SetActive(false);
            aimCamera.SetActive(true);
            StartCoroutine(ShowReticle());
            isAiming = true;

            // Crear la flecha en la posición del arco
            flechaActual = Instantiate(arrowPrefab, bowPosition.transform.position, bowPosition.transform.rotation);
            flechaActual.transform.SetParent(bowPosition.transform);
            flechaActual.GetComponent<Rigidbody>().isKinematic = true; // Desactiva la física temporalmente
            player.anim.Play("shoot");
        }
        if (Input.GetMouseButton(1))
        {
            // Aumentar el tiempo de carga mientras se mantiene el botón derecho
            tiempoDeApuntar += Time.deltaTime;
        }

        // Manejando el disparo (botón izquierdo del ratón)
        if (isAiming && Input.GetMouseButtonDown(0))
        {
            player.anim.Play("recoil");
            isAiming = false;
            ShootArrow();
            mainCamera.SetActive(true);
            aimCamera.SetActive(false);
            StartCoroutine(HideReticle());
        }
        if (Input.GetMouseButtonUp(1) && isAiming)
        {
            Destroy(flechaActual); // Elimina la flecha sin dispararla
            isAiming = false;
            player.anim.Play("recoil");
            mainCamera.SetActive(true);
            aimCamera.SetActive(false);
            StartCoroutine(HideReticle());
        }
    }

    IEnumerator ShowReticle()
    {
        yield return new WaitForSeconds(0.25f);
        aimReticle.enabled = true; // Activa la retícula
    }
    IEnumerator HideReticle()
    {
        yield return new WaitForSeconds(0.25f);
        aimReticle.enabled = false; 
    }

    void ShootArrow()
    {


        // Obtener el componente ArrowProjectile y ejecutar la función de disparo
        flechaActual.transform.SetParent(null);
        Flecha codigoFlecha = flechaActual.GetComponent<Flecha>();
        if (codigoFlecha != null)
        {
            codigoFlecha.Fire();
        }
    }
}



