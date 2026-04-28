using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Nueva Arma", menuName = "Arma")]
public class ArmaData : ScriptableObject
{

    [Header("Info General")]
    public string nombre;

    [Header("Dano")]
    public int danoGolpeFuerte;
    public int danoGolpeLigero;
    public int danoGolpeCargado;
    public int danoGolpeFuerteGuardia;
    public int danoGolpeLigeroGuardia;

    [Header("Prefabs de Arma")]
    public GameObject prefabArmaPrincipal;
    public GameObject prefabArmaSecundaria;

}
