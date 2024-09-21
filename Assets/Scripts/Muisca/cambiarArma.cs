using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cambiarArma : MonoBehaviour
{
    [SerializeField] GameObject armaMelee;
    [SerializeField] GameObject armaDistancia;

    void Start()
    {
        armaMelee.SetActive(true);
        armaDistancia.SetActive(false);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            armaMelee.SetActive(true); 
            armaDistancia.SetActive(false);  
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            armaMelee.SetActive(false);  
            armaDistancia.SetActive(true);
        }
    }
}

