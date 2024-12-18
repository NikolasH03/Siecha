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
    public float dañoBase;
    public float curacionEstamina;
    public Image imagenBarraVida;
    public Image imagenEstamina;
    public float tiempoUltimoRelleno; // Variable para rastrear el tiempo del último relleno
    public bool dañoEstamina;
    public static HealthBar instance;
    public float tiempoParry;

    //variables para salida forzosa del bloqueo
    public float tiempoDeBloqueo = 10f; // Tiempo en segundos para bloquear la entrada


    //Variables para despues de morir
    public bool playerDied;

    // otros codigos necesarios
    //[SerializeField] cambiarPersona protagonista;
    //[SerializeField] Player protagonista1;
    private Player protagonista;

    AudioManager audioManager;

    public bool detectaAtaque;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
    
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

        playerDied = false;

    }
    private void Update()
    {
        protagonista = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

 
        float tiempoTranscurrido = Time.time - tiempoUltimoRelleno;
        if(estaminaActual < 0)
        {
            estaminaActual = 0;
        }

        if (tiempoTranscurrido >= 4f)
        {
     
            float estaminaPorSegundo = curacionEstamina / 2f;

         
            float estaminaRelleno = estaminaPorSegundo * Mathf.Floor(tiempoTranscurrido / 4f);

       
            estaminaActual += estaminaRelleno;
            estaminaActual = Mathf.Min(estaminaActual, estaminaMax);

            
            imagenEstamina.fillAmount = estaminaActual / estaminaMax;

           
            tiempoUltimoRelleno = Time.time;
        }


        if (detectaAtaque)
        {
            controladorDaño();
        }
        
       
        
       
    }


    public void controladorDaño()
    {
        
                if (protagonista.anim.GetBool("blocking"))
                {
                    Debug.Log("Tiempo: " + Mathf.FloorToInt(protagonista.tiempoTranscurrido * 1000) + " milisegundos");
                    tiempoParry = protagonista.tiempoTranscurrido * 1000;

                    if (tiempoParry <= 4)
                    {
                        protagonista.anim.Play("parry");
                        protagonista.ResetTimer();
                        detectaAtaque = false;
                    }
                    else
                    {
                    
                  
                    if (estaminaActual > 0)
                        {

                            estaminaActual -= dañoBase;
                            imagenEstamina.fillAmount = estaminaActual / estaminaMax;
                            protagonista.anim.Play("daño_bloqueando");
                            audioManager.playAudio(audioManager.block);
                            protagonista.GetComponent<Collider>().enabled = false;
                            protagonista.GetComponent<Rigidbody>().isKinematic = true;
                            detectaAtaque = false;


                    }


                        if (estaminaActual <= 0)
                        {
                            protagonista.anim.Play("daño");
                        audioManager.playAudio(audioManager.damage);
                        protagonista.GetComponent<Collider>().enabled = true;
                    protagonista.GetComponent<Rigidbody>().isKinematic = false;
                    detectaAtaque = false;
                          
                    }
                    }




                }
                else
                {
               
               
                    protagonista.canMove = false;
                    vidaActual -= dañoBase;
                    imagenBarraVida.fillAmount = vidaActual / vidaMax;

                    if (vidaActual <= 0)
                    {

                        protagonista.GetComponent<Collider>().enabled = false;
                protagonista.GetComponent<Rigidbody>().isKinematic = true;
                protagonista.anim.Play("morir");
                    audioManager.playAudio(audioManager.death);
                    detectaAtaque = false;


                }
                    else
                    {
                        protagonista.GetComponent<Collider>().enabled = false;
                protagonista.GetComponent<Rigidbody>().isKinematic = true;
                protagonista.anim.Play("daño");
                    audioManager.playAudio(audioManager.damage);
                    detectaAtaque = false;

                }

                }
            
                    
      
    }
   
   



}
