using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flecha : MonoBehaviour
{
    public GameObject prefabFlecha;
    public Transform puntoDeDisparo;
    [SerializeField] public DetectarJugador detectarJugador;
    
    public void DispararFlecha()
    {
        Instantiate(prefabFlecha, puntoDeDisparo.position, Quaternion.LookRotation(detectarJugador.Player.position - puntoDeDisparo.position));
    }

}
