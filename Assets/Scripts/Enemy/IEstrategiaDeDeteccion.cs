using UnityEngine;

public interface IEstrategiaDeDeteccion
{
    bool Ejecutar(Transform jugador, Transform detector, Temporizador temporizador);
}