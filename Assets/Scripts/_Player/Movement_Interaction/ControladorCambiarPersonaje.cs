using Cinemachine;
using UnityEngine;

public class ControladorCambiarPersonaje : MonoBehaviour
{
    //singlenton
    public static ControladorCambiarPersonaje instance;

    //objetos necesarios y sus derivados
    [SerializeField] private GameObject muisca;
    [SerializeField] private GameObject espanol;
    [SerializeField] private CinemachineVirtualCamera camaraPrincipal;
    [SerializeField] private CinemachineVirtualCamera camaraApuntado;
    [SerializeField] private GameObject HUDMuisca;
    [SerializeField] private GameObject HUDEspanol;
    private ControladorCombate controladorMuisca;
    private ControladorCombate controladorEspanol;
    private Transform objetivoCamaraMuisca;
    private Transform objetivoCamaraEspanol;

    //logica para cambiar el personaje
    private bool esMuisca;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        controladorMuisca = muisca.GetComponent<ControladorCombate>();
        controladorEspanol = espanol.GetComponent<ControladorCombate>();

        objetivoCamaraMuisca = muisca.transform.Find("camaraTarget");
        objetivoCamaraEspanol = espanol.transform.Find("camaraTarget");
    }
    void Start()
    {
        activarEspanol();
    }

    public void CambiarProtagonista()
    {
        if (espanol.activeSelf)
        {
            activarMuisca();
        }

        else
        {
            activarEspanol();
        }

    }
    public void activarMuisca()
    {
        muisca.transform.position = espanol.transform.position;
        muisca.transform.rotation = espanol.transform.rotation;
        espanol.SetActive(false);
        muisca.SetActive(true);
        camaraPrincipal.Follow = objetivoCamaraMuisca;
        camaraApuntado.Follow = objetivoCamaraMuisca;
        HUDEspanol.SetActive(false);
        HUDMuisca.SetActive(true);

        esMuisca = true;
    }
    public void activarEspanol()
    {
        espanol.transform.position = muisca.transform.position;
        espanol.transform.rotation = muisca.transform.rotation;

        muisca.SetActive(false);
        espanol.SetActive(true);
        camaraPrincipal.Follow = objetivoCamaraEspanol;
        camaraApuntado.Follow = objetivoCamaraEspanol;
        HUDEspanol.SetActive(true);
        HUDMuisca.SetActive(false);

        esMuisca = false;
    }

    public bool getEsMuisca()
    {
        return esMuisca;
    }
}
