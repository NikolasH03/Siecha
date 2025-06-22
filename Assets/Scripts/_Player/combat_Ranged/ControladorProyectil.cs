using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorProyectil : MonoBehaviour
{
    private Rigidbody proyectilRB;
    HealthbarEnemigo enemigo;
    ControladorCombate player;
    private void Awake()
    {
        proyectilRB = GetComponent<Rigidbody>();
    }

    void Start()
    {
        float velocidad  = 20f;
        proyectilRB.velocity = transform.forward*velocidad;

    }
    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCombate>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "enemy")
        {

            enemigo = other.GetComponent<HealthbarEnemigo>();
            enemigo.recibeDaño(player.EntregarDañoArmaDistancia());
            enemigo.setRecibiendoDaño(true);
        }
        Destroy(gameObject);
    }

}
