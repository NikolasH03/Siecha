using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public int vidaMax;
    public float vidaActual;
    public int estaminaMax;
    public float estaminaActual;
    public float dañoBase = 3;
    public float curacionEstamina = 2;
    public Image imagenBarraVida;
    public Image imagenEstamina;
    [SerializeField] Player player;
    public float tiempoUltimoRelleno; // Variable para rastrear el tiempo del último relleno
    public bool dañoEstamina;
    public static HealthBar instance;

    //variables para salida forzosa del bloqueo
    public float tiempoDeBloqueo = 10f; // Tiempo en segundos para bloquear la entrada
    public MonoBehaviour[] componentesAEsperarEntrada; // Componentes que manejan la entrada del usuario
    private bool bloqueoActivo = false;

    //Variables para despues de morir
    public bool playerDied;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        vidaActual = vidaMax;
        estaminaActual = estaminaMax;

        // Establecer el tiempo del último relleno al inicio
        tiempoUltimoRelleno = Time.time;

        playerDied = false;

    }
    private void Update()
    {
        // Calcular el tiempo transcurrido desde el último relleno
        float tiempoTranscurrido = Time.time - tiempoUltimoRelleno;
        if(estaminaActual < 0)
        {
            estaminaActual = 0;
        }
        // Si ha pasado al menos 4 segundos desde el último relleno
        if (tiempoTranscurrido >= 4f)
        {
            // Calcular la cantidad de estamina que se debe rellenar cada segundo
            float estaminaPorSegundo = curacionEstamina / 4f;

            // Calcular cuánta estamina debería haberse rellenado desde el último relleno
            float estaminaRelleno = estaminaPorSegundo * Mathf.Floor(tiempoTranscurrido / 4f);

            // Incrementar la estamina actual y asegurarse de que no supere el máximo
            estaminaActual += estaminaRelleno;
            estaminaActual = Mathf.Min(estaminaActual, estaminaMax);

            // Actualizar la barra de estamina
            imagenEstamina.fillAmount = estaminaActual / estaminaMax;

            // Actualizar el tiempo del último relleno
            tiempoUltimoRelleno = Time.time;
        }
        
       
        
       
    }


    public void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag == "hostile")
        {
            if (player.anim.GetBool("blocking"))
            {

                if (estaminaActual > 0)
                {
                    estaminaActual -= dañoBase;
                    imagenEstamina.fillAmount = estaminaActual / estaminaMax;
                    player.anim.Play("Standing Block React");
                    Player.instance.GetComponent<Collider>().enabled = false;
                    
                    

                }
                

                if (estaminaActual <= 0)
                {      
                    player.anim.Play("Damage");
                    Player.instance.GetComponent<Collider>().enabled = true;
                }

            }
            else
            {
   
                player.canMove = false;
                vidaActual -= dañoBase;
                imagenBarraVida.fillAmount = vidaActual / vidaMax;

                if (vidaActual <= 0)
                {

                    Player.instance.GetComponent<Collider>().enabled = false;
                    player.anim.Play("Death Forward");
                   


                }
                else
                {
                    Player.instance.GetComponent<Collider>().enabled = false;
                    player.anim.Play("Damage");
                   
                }
                
            }
           

           
        }
      
    }
    public void playerDead()
    {
        playerDied=true;
        player.canMove = true;
    }
    public void FinishDamage()
    {
        Player.instance.GetComponent<Collider>().enabled = true;
        player.canMove = true;
    }
   



}
