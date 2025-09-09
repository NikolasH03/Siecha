using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorCambioArmas : MonoBehaviour
{
    public List<GameObject> armasMelee;
    [SerializeField] private GameObject armaDistancia;
    [SerializeField] private int numeroArma;

    private ControladorCombate controladorCombate;
    private void Awake()
    {
        controladorCombate = GetComponent<ControladorCombate>();
    }
    private void Start()
    {
        armasMelee = controladorCombate.getArmaActual();
        CambiarArmaMelee();
    }

    public void CambiarArmaMelee()
    {
        SetListaActiva(armasMelee, true);
        armaDistancia.SetActive(false);
        numeroArma = 1;
    }

    public void CambiarArmaDistancia()
    {
        SetListaActiva(armasMelee, false);
        armaDistancia.SetActive(true);
        numeroArma = 2;
    }

    public int getterArma()
    {
        return numeroArma;
    }
    private void SetListaActiva(List<GameObject> lista, bool estado)
    {
        foreach (GameObject arma in lista)
        {
            if (arma != null)
                arma.SetActive(estado);
        }
    }
}

