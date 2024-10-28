using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class logicaEnemigo : MonoBehaviour
{
    [SerializeField] float vidaActual;
    [SerializeField] float vidaMax;
    [SerializeField] int dañoLigero;
    [SerializeField] int dañoFuerte;
    [SerializeField] string tipoDaño;
    [SerializeField] Animator anim;
    [SerializeField] Image imagenBarraVida;
    [SerializeField] Money dinero;
    public static logicaEnemigo instance;
    public void Start()
    {
        vidaActual = vidaMax;
    }
    private void Awake()
    {
        instance = this;
    }
    public void OnTriggerEnter(Collider other)
    {

        Debug.Log("detecta un collider");
       if (other.gameObject.tag == "arma")
       {
            Debug.Log("detecta el collider del arma");
            if (tipoDaño == "ligero")
            {
                vidaActual -= dañoLigero;
            }
            else if (tipoDaño == "fuerte")
            {
                vidaActual -= dañoFuerte;
            }
           

            imagenBarraVida.fillAmount = vidaActual / vidaMax;

            if (vidaActual <= 0)
            {
                GetComponent<Collider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                anim.Play("Falling Back Death");
                dinero.contador = dinero.contador + 50;
                float delayInSeconds = 2.0f; 
                Invoke("eliminarEnemigo", delayInSeconds);
            }
            else
            {
                anim.Play("damage");

            }


        }

        
    }
    public void eliminarEnemigo()
    {
        gameObject.SetActive(false);
    }
    public void tipoDeDaño(string ataque)
    {
        tipoDaño= ataque;
       
    }

}
