using Cinemachine;
using UnityEngine;

public class ControladorCambiarPersonaje : MonoBehaviour
{
    public static ControladorCambiarPersonaje instance;

    [SerializeField] private GameObject muisca;
    [SerializeField] private GameObject espanol;
    [SerializeField] private CinemachineVirtualCamera camaraPrincipal;
    [SerializeField] private CinemachineVirtualCamera camaraApuntado;
    [SerializeField] private GameObject HUDMuisca;
    [SerializeField] private GameObject HUDEspanol;
    private Transform objetivoCamaraMuisca;
    private Transform objetivoCamaraEspanol;

    private bool esMuisca;
    public bool PuedePausar = false;

    [SerializeField] EnemyManager enemigos;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        objetivoCamaraMuisca = muisca.transform.Find("camaraTarget");
        objetivoCamaraEspanol = espanol.transform.Find("camaraTarget");
        activarMuisca();
        enemigos?.ActualizarJugador();
    }

    public void CambiarProtagonista()
    {
        if (espanol.activeSelf)
            activarMuisca();
        else
            activarEspanol();

        enemigos?.ActualizarJugador();
        
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
        
        muisca.GetComponent<ControladorCombate>().AlActivarse();
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
        
        espanol.GetComponent<ControladorCombate>().AlActivarse();
    }

    public void OcultarTodosLosHUD()
    {
        HUDEspanol.SetActive(false);
        HUDMuisca.SetActive(false);
    }

    public void ActivarHUDPausa()
    {
        if (esMuisca)
            HUDMuisca.SetActive(true);
        else
            HUDEspanol.SetActive(true);
    }

    public bool getEsMuisca()
    {
        return esMuisca;
    }
}