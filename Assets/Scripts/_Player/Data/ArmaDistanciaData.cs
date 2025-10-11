using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Nueva Arma Distancia", menuName = "Arma Distancia")]
public class ArmaDistanciaData : ScriptableObject
{

    public string nombre;

    public int danoDisparo;
    public int dañoDisparoGuardia;

    public GameObject prefab;


}
