using Cinemachine;
using UnityEngine;

public class ControladorCambiarPersonaje : MonoBehaviour
{
    public static ControladorCambiarPersonaje instance;

    [SerializeField] GameObject muisca;
    [SerializeField] GameObject espanol;
    [SerializeField] CinemachineVirtualCamera camaraPrincipal;
    [SerializeField] CinemachineVirtualCamera camaraApuntado;
    [SerializeField] ControladorCombate controladorMuisca;
    [SerializeField] ControladorCombate controladorEspanol;
    [SerializeField] Transform objetivoCamaraMuisca;
    [SerializeField] Transform objetivoCamaraEspanol;

    [SerializeField] bool protagonistaUno;

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
        


        muisca.SetActive(false);
        espanol.SetActive(true);
        camaraPrincipal.Follow = objetivoCamaraEspanol;
        camaraApuntado.Follow = objetivoCamaraEspanol;

        protagonistaUno = false;

    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.G) && espanol.activeSelf && !controladorEspanol.getAtacando())
        {
            activarMuisca();
        }

        else if (Input.GetKeyDown(KeyCode.G) && muisca.activeSelf && !controladorMuisca.getAtacando())
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

        protagonistaUno = true;
    }
    public void activarEspanol()
    {
        espanol.transform.position = muisca.transform.position;
        espanol.transform.rotation = muisca.transform.rotation;

        muisca.SetActive(false);
        espanol.SetActive(true);
        camaraPrincipal.Follow = objetivoCamaraEspanol;
        camaraApuntado.Follow = objetivoCamaraEspanol;

        protagonistaUno = false;
    }

}
