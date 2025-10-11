using UnityEngine;

public class EstrategiaDeDeteccionCono : IEstrategiaDeDeteccion
{
    private readonly float _angulo;
    private readonly float _radio;
    private readonly float _radioInternoDeDeteccion;

    public EstrategiaDeDeteccionCono(float angulo, float radioExterno, float radioInternoDeDeteccion)
    {
        this._angulo = angulo;
        this._radio = radioExterno;
        this._radioInternoDeDeteccion = radioInternoDeDeteccion;
    }

    public bool Ejecutar(Transform player, Transform detector, Temporizador temporizador)
    {
        if (temporizador.EstaCorriendo) return false;

        var direccionAJugador = player.position - detector.position;
        var distancia = direccionAJugador.magnitude;
        var anguloAJugador = Vector3.Angle(direccionAJugador, detector.forward);

        // Si está en el radio interno, detectar siempre
        if (distancia <= _radioInternoDeDeteccion)
        {
            temporizador.Empezar();
            return true;
        }

        // Si no está en el radio interno, comprobar cono de visión
        if (distancia <= _radio && anguloAJugador <= _angulo / 2f)
        {
            temporizador.Empezar();
            return true;
        }

        return false;
    }

}