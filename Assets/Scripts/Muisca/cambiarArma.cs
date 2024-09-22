using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cambiarArma : MonoBehaviour
{
    [SerializeField] GameObject armaMelee;
    [SerializeField] GameObject armaDistancia;
    private int numeroArma;

    void Start()
    {
        armaMelee.SetActive(true);
        armaDistancia.SetActive(false);
        numeroArma = 1;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            armaMelee.SetActive(true); 
            armaDistancia.SetActive(false);
            numeroArma = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            armaMelee.SetActive(false);  
            armaDistancia.SetActive(true);
            numeroArma = 2;
        }
    }
    public int getterArma()
    {
        return numeroArma;
    }
}

