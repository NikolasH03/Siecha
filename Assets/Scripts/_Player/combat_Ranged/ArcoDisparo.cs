using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcoDisparo : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    private Rigidbody proyectilRB;
    private float velocidad = 80f;

    [Header("Configuración de Colisiones")]
    [SerializeField] private LayerMask capasDeteccion = -1; 

    [Header("Configuración de Destrucción")]
    [SerializeField] private float tiempoVidaMaximo = 5f;
    [SerializeField] private float distanciaMaxima = 150f;
    [SerializeField] private float alturaMaxima = 50f;


    private HealthComp enemigo;
    private ControladorCombate player;
    private Vector3 posicionInicial;
    private float tiempoInicio;
    private bool yaImpacto = false;

    private void Awake()
    {
        proyectilRB = GetComponent<Rigidbody>();

        gameObject.layer = LayerMask.NameToLayer("Proyectil");

        posicionInicial = transform.position;
        tiempoInicio = Time.time;
    }

    void Start()
    {
        proyectilRB.velocity = transform.forward * velocidad;

        Destroy(gameObject, tiempoVidaMaximo);

        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            player = jugadorObj.GetComponent<ControladorCombate>();
        }
    }

    private void Update()
    {
        VerificarLimitesDestruccion();
    }

    private void VerificarLimitesDestruccion()
    {
        if (yaImpacto) return;

        float distanciaRecorrida = Vector3.Distance(transform.position, posicionInicial);
        if (distanciaRecorrida > distanciaMaxima)
        {
            Debug.Log($"Flecha destruida por distancia máxima: {distanciaRecorrida}m");
            DestruirFlecha();
            return;
        }

        if (transform.position.y > alturaMaxima)
        {
            Debug.Log($"Flecha destruida por altura máxima: {transform.position.y}m");
            DestruirFlecha();
            return;
        }

        if (transform.position.y < posicionInicial.y - 20f)
        {
            Debug.Log("Flecha destruida por caer al vacío");
            DestruirFlecha();
            return;
        }

        if (proyectilRB.velocity.magnitude < 1f && Time.time - tiempoInicio > 1f)
        {
            Debug.Log("Flecha destruida por velocidad muy baja (atascada)");
            DestruirFlecha();
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaImpacto) return;

        int otherLayer = 1 << other.gameObject.layer;
        if ((capasDeteccion.value & otherLayer) == 0)
        {
            return; 
        }

        if (other.gameObject.CompareTag("enemy"))
        {
            ManejarImpactoEnemigo(other);
        }

        else
        {
            ManejarImpactoObstaculo(other);
        }
    }

    private void ManejarImpactoEnemigo(Collider enemyCollider)
    {
        yaImpacto = true;

        if (player != null)
        {
            enemigo = enemyCollider.GetComponent<HealthComp>();
            if (enemigo != null)
            {
                enemigo.recibeDano(player.EntregarDanoArmaDistancia());
                enemigo.setRecibiendoDano(true);

                Debug.Log($"Flecha impactó enemigo: {enemyCollider.gameObject.name}");
            }
        }

        // ReproducirEfectoImpactoEnemigo();

        DestruirFlecha();
    }

    private void ManejarImpactoObstaculo(Collider obstaculoCollider)
    {
        yaImpacto = true;

        Debug.Log($"Flecha impactó obstáculo: {obstaculoCollider.gameObject.name}");

        // ReproducirEfectoImpactoObstaculo();

        ClavaEnPared(obstaculoCollider);

        //DestruirFlecha();
    }

    private void DestruirFlecha()
    {
        if (proyectilRB != null && !proyectilRB.isKinematic)
        {
            proyectilRB.velocity = Vector3.zero;
            proyectilRB.angularVelocity = Vector3.zero;
            proyectilRB.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Reproducir sonido de impacto
        // AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position);

        Destroy(gameObject);
    }

    private void ClavaEnPared(Collider pared)
    {

        proyectilRB.velocity = Vector3.zero;
        proyectilRB.isKinematic = true;

        if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit, 1f))
        {
            transform.rotation = Quaternion.LookRotation(-hit.normal);
        }

        Destroy(gameObject, 2f);
    }
}
