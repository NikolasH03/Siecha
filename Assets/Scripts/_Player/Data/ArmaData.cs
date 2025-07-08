using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Nueva Arma", menuName = "Arma")]
public class ArmaData : ScriptableObject
{

    public string nombre;

    public int danoGolpeFuerte;
    public int danoGolpeLigero;
    public int danoGolpeFuerteGuardia;
    public int danoGolpeLigeroGuardia;

    public GameObject prefab;


}
