using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorCombate : MonoBehaviour
{

    public Animator anim;

    //ataque
    [SerializeField] int numeroArma;
    [SerializeField] bool atacando = false;
    public int numeroGolpesLigeros   = 0;
    public int numeroGolpesFuertes = 0;
    [SerializeField] Collider ColliderArma;
    [SerializeField] Collider ColliderPierna;




    //referencias 
    [SerializeField] ControladorCambioArmas cambioArma;
    ControladorMovimiento controladorMovimiento;
    //[SerializeField] HabilidadesJugador habilidadesJugador;
    private void Start()
    {


        ColliderArma.enabled = false;
        ColliderPierna.enabled = false;


        anim = GetComponent<Animator>();
        controladorMovimiento = GetComponent<ControladorMovimiento>();
    }
    public void Update()
    {
        golpeCheck();
        bloqueoCheck();
        dashCheck();
    }



    //verifica si el usuario oprimio el click y activa la animacion de golpe
    public void golpeCheck()
    {
        numeroArma = cambioArma.getterArma();

        if (numeroArma == 1)
        {

            if (Input.GetMouseButton(0) && !atacando && !InputJugador.instance.correr)
            {

                atacando = true;
                numeroGolpesLigeros = 1;

            }
            if (Input.GetMouseButton(1) && !atacando && !InputJugador.instance.correr)
            {

                atacando = true;
                numeroGolpesFuertes = 1;

            }
            if (!atacando)
            {
                numeroGolpesLigeros = 0;
                numeroGolpesFuertes = 0;
            }

        }



    }


    //verifica si el jugador esta manteniendo oprimida la tecla para bloquear
    public void bloqueoCheck()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetBool("blocking", true);
        }

        else if (Input.GetKeyUp(KeyCode.Space))
        {
            GetComponent<Collider>().enabled = true;
            anim.SetBool("blocking", false);
        }

    }

    public void dashCheck()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetBool("dashing", true);
        }
    }

    public void terminarDash()
    {
        anim.SetBool("dashing", false);
        controladorMovimiento.GetComponent<Collider>().enabled = true;
        controladorMovimiento.GetComponent<Rigidbody>().isKinematic = false;
    }
    public void bloquearDespuesDeGolpe()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public bool PuedeUsarCapoeira()
    {
        return HabilidadesJugador.instance.estaDesbloqueada(HabilidadesJugador.TipoHabilidad.Capoeira);
    }


    //activar colliders de diferentes armas
    public void activarColliderArma()
    {
        ColliderArma.enabled = true;
    }
    public void desactivarColliderArma()
    {
        ColliderArma.enabled = false;
    }
    public void activarColliderPierna()
    {
        ColliderPierna.enabled = true;
    }
    public void desactivarColliderPierna()
    {
        ColliderPierna.enabled = false;
    }
    public bool getAtacando()
    {
        return atacando;
    }
    public void setAtacando(bool ataque)
    {
        atacando = ataque;
    }

 


}
