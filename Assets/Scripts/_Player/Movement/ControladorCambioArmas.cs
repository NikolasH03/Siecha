using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorCambioArmas : MonoBehaviour
{
    [SerializeField] GameObject armaMelee;
    [SerializeField] GameObject armaDistancia;
    private int numeroArma;

    ControladorCombate controladorCombate;
    void Start()
    {
        armaMelee.SetActive(true);
        armaDistancia.SetActive(false);
        numeroArma = 1;
    }

    void Update()
    {
        controladorCombate = GetComponent<ControladorCombate>();
        armaMelee = controladorCombate.getArmaActual();


    }

    public void CambiarArmaMelee()
    {
        armaMelee.SetActive(true);
        armaDistancia.SetActive(false);
        numeroArma = 1;
    }
    public void CambiarArmaDistancia()
    {
        armaMelee.SetActive(false);
        armaDistancia.SetActive(true);
        numeroArma = 2;
        controladorCombate.setAtacando(false);
    }
    public int getterArma()
    {
        return numeroArma;
    }
}

