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
    private Transform objetivoCamaraMuisca;
    private Transform objetivoCamaraEspanol;

    //logica para cambiar el personaje
    private bool esMuisca;
    public bool PuedePausar = false;

    //otras referencias
    [SerializeField] EnemyManager enemigos;

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

        objetivoCamaraMuisca = muisca.transform.Find("camaraTarget");
        objetivoCamaraEspanol = espanol.transform.Find("camaraTarget");
        activarMuisca();
        enemigos.ActualizarJugador();
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
        enemigos.ActualizarJugador();
        SincronizarInputConPersonajeActivo();
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
    public void OcultarTodosLosHUD()
    {
        HUDEspanol.SetActive(false);
        HUDMuisca.SetActive(false);
    }
    public void ActivarHUDPausa()
    {
        if (esMuisca)
        {
            HUDMuisca.SetActive(true);
        }
        else
        {
            HUDEspanol.SetActive(true);
        }
    }
    private void SincronizarInputConPersonajeActivo()
    {
        GameObject personajeActivo = esMuisca ? muisca : espanol;
        ControladorCambioArmas controladorArmas = personajeActivo.GetComponent<ControladorCambioArmas>();

        if (controladorArmas != null)
        {
            int armaActual = controladorArmas.getterArma();
            //Debug.Log($"Sincronizando input para {(esMuisca ? "Muisca" : "Español")} - Arma: {armaActual}");

            if (armaActual == 1)
            {
                InputJugador.instance.CambiarInputMelee();
            }
            else if (armaActual == 2)
            {
                InputJugador.instance.CambiarInputDistancia();
            }
        }
    }
    public bool getEsMuisca()
    {
        return esMuisca;
    }
}
