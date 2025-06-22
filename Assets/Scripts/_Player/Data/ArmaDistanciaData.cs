using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Nueva Arma Distancia", menuName = "Arma Distancia")]
public class ArmaDistanciaData : ScriptableObject
{

    public string nombre;

    public int dañoDisparo;
    public int dañoDisparoGuardia;

    public GameObject prefab;


}
