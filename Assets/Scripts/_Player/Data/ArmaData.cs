using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Nueva Arma", menuName = "Arma")]
public class ArmaData : ScriptableObject
{

    [Header("Info General")]
    public string nombre;

    [Header("Daño")]
    public int dañoGolpeFuerte;
    public int dañoGolpeLigero;
    public int dañoGolpeCargado;
    public int dañoGolpeFuerteGuardia;
    public int dañoGolpeLigeroGuardia;

    [Header("Prefabs de Arma")]
    public GameObject prefabArmaPrincipal;
    public GameObject prefabArmaSecundaria;

}
