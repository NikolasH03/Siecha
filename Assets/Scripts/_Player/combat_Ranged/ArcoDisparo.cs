using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcoDisparo : MonoBehaviour
{
    private Rigidbody proyectilRB;
    private float velocidad = 50f;
    private HealthbarEnemigo enemigo;
    private ControladorCombate player;
    private void Awake()
    {
        proyectilRB = GetComponent<Rigidbody>();
    }
    void Start()
    {
        proyectilRB.velocity = transform.forward*velocidad;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "enemy")
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCombate>();
            enemigo = other.GetComponent<HealthbarEnemigo>();
            enemigo.recibeDaño(player.EntregarDañoArmaDistancia());
            enemigo.setRecibiendoDaño(true);
        }
        Destroy(gameObject);
    }

}
