using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthbarEnemigo : MonoBehaviour
{
    [SerializeField] float vidaActual;
    [SerializeField] float vidaMax;
    [SerializeField] Animator anim;
    [SerializeField] Image imagenBarraVida;
    [SerializeField] bool EnemigoMuerto;
    [SerializeField] bool RecibiendoDaño;
    public void Start()
    {
        vidaActual = vidaMax;
        EnemigoMuerto = false;
    }

    public void Update()
    {
           
           imagenBarraVida.fillAmount = vidaActual / vidaMax;

        if (RecibiendoDaño == true)
        {
            if (vidaActual <= 0 && !EnemigoMuerto)
            {
                GetComponent<Collider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                anim.Play("Falling Back Death");
                float delayInSeconds = 2.0f;
                Invoke("eliminarEnemigo", delayInSeconds);
                EnemigoMuerto = true;
                RecibiendoDaño = false;
            }
            else
            {
                anim.Play("damage");
                RecibiendoDaño = false;

            }

        }
            

    }
    public void recibeDaño(int daño)
    {
        vidaActual -= daño;
    }
    public void eliminarEnemigo()
    {

        InventarioEconomia.instance.enemigoMuerto(1);
        Debug.Log("llama al inventario");
        gameObject.SetActive(false);  

    }

    public void setRecibiendoDaño(bool recibiendoDaño)
    {
        RecibiendoDaño = recibiendoDaño;
    }


}
