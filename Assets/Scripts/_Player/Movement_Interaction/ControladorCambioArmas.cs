using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorCambioArmas : MonoBehaviour
{
    [SerializeField] private GameObject armaMelee;
    [SerializeField] private GameObject armaDistancia;
    private int numeroArma;

    private ControladorCombate controladorCombate;
    private void Awake()
    {
        controladorCombate = GetComponent<ControladorCombate>();
    }
    private void Start()
    {
        armaMelee = controladorCombate.getArmaActual();
        armaMelee.SetActive(true);
        armaDistancia.SetActive(false);
        numeroArma = 1;

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

