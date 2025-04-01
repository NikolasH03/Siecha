using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public static HealthBar instance;

    [SerializeField] int vidaMax;
    [SerializeField] float vidaActual;
    [SerializeField] int estaminaMax;
    [SerializeField] float estaminaActual;
    [SerializeField] bool recibeDaño;
    [SerializeField] float dañoBase;
    [SerializeField] float curacionEstamina;
    [SerializeField] Image imagenBarraVida;
    [SerializeField] Image imagenEstamina;
    [SerializeField] float tiempoUltimoRelleno;


    [SerializeField] bool jugadorMuerto;
    [SerializeField] TextMeshProUGUI canvasNumMuertes;
    [SerializeField] int contadorMuertes = 0;
    [SerializeField] GameObject menuMuerte;

    private ControladorMovimiento controladorMovimiento;


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
    }
    void Start()
    {
        vidaActual = vidaMax;
        estaminaActual = estaminaMax;

        tiempoUltimoRelleno = Time.time;

        menuMuerte.SetActive(false);
        canvasNumMuertes.text = contadorMuertes.ToString();

        recibeDaño = false;
        jugadorMuerto = false;

    }
    private void Update()
    {
        controladorMovimiento = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorMovimiento>();


        float tiempoTranscurrido = Time.time - tiempoUltimoRelleno;
        if (estaminaActual < 0)
        {
            estaminaActual = 0;
        }

        if (tiempoTranscurrido >= 2f)
        {
            rellenarBarraEstamina(tiempoTranscurrido);
        }


        if (recibeDaño)
        {
            controladorDaño();
        }
        if (jugadorMuerto)
        {
            terminoAnimacionMuerte();
        }




    }


    public void controladorDaño()
    {

        if (controladorMovimiento.getAnim().GetBool("blocking"))
        {

            if (estaminaActual > 0)
            {
                recibeDañoBloqueo();
            }


            if (estaminaActual <= 0)
            {
                rompeBloqueo();
            }
        }

        else
        {

            controladorMovimiento.setCanMove(false);
            vidaActual -= dañoBase;
            imagenBarraVida.fillAmount = vidaActual / vidaMax;

            if (vidaActual <= 0)
            {
                jugadorMuere();
            }
            else
            {
                recibeDañoVida();
            }

        }
    }

    public void rellenarBarraEstamina(float tiempoTranscurrido)
    {
        float estaminaPorSegundo = curacionEstamina / 2f;
        float estaminaRelleno = estaminaPorSegundo * Mathf.Floor(tiempoTranscurrido / 2f);

        estaminaActual += estaminaRelleno;
        estaminaActual = Mathf.Min(estaminaActual, estaminaMax);

        imagenEstamina.fillAmount = estaminaActual / estaminaMax;

        tiempoUltimoRelleno = Time.time;
    }

    public void recibeDañoBloqueo()
    {
        estaminaActual -= dañoBase;
        imagenEstamina.fillAmount = estaminaActual / estaminaMax;
        controladorMovimiento.getAnim().Play("daño_bloqueando");
        ControladorSonido.instance.playAudio(ControladorSonido.instance.block);
        controladorMovimiento.GetComponent<Collider>().enabled = false;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = true;
        recibeDaño = false;
    }

    public void rompeBloqueo()
    {
        controladorMovimiento.getAnim().Play("daño");
        ControladorSonido.instance.playAudio(ControladorSonido.instance.damage);
        controladorMovimiento.GetComponent<Collider>().enabled = true;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = false;
        recibeDaño = false;
    }

    public void recibeDañoVida()
    {
        controladorMovimiento.GetComponent<Collider>().enabled = false;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = true;
        controladorMovimiento.getAnim().SetBool("running", false);
        controladorMovimiento.getAnim().Play("daño");
        ControladorSonido.instance.playAudio(ControladorSonido.instance.damage);
        recibeDaño = false;
    }

    public void jugadorMuere()
    {
        controladorMovimiento.GetComponent<Collider>().enabled = false;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = true;
        controladorMovimiento.getAnim().Play("morir");
        ControladorSonido.instance.playAudio(ControladorSonido.instance.death);
        recibeDaño = false;
    }

    public void terminoAnimacionMuerte()
    {
        controladorMovimiento.getAnim().speed = 0;
        menuMuerte.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        contadorMuertes++;
        canvasNumMuertes.text = contadorMuertes.ToString();
        jugadorMuerto = false;
    }

    public void revivirJugador()
    {
        controladorMovimiento.getAnim().speed = 1;
        menuMuerte.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        vidaActual = vidaMax;
        estaminaActual = estaminaMax;
        tiempoUltimoRelleno = Time.time;
        imagenBarraVida.fillAmount = vidaMax;
        imagenEstamina.fillAmount = estaminaMax;

        controladorMovimiento.GetComponent<Collider>().enabled = true;
    }

    // setters y getters

    public float getVidaActual()
    {
        return vidaActual;
    }
    public void setVidaActual(float vida)
    {
        vidaActual= vida;
    }

    public bool getRecibeDaño()
    {
        return recibeDaño;
    }
    public void setRecibeDaño(bool boolDaño)
    {
        recibeDaño = boolDaño;
    }
    public bool getJugadorMuerto()
    {
        return jugadorMuerto;
    }
    public void setJugadorMuerto(bool boolMuerto)
    {
        jugadorMuerto = boolMuerto;
    }




}
