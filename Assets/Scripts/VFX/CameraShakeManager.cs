using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    [Header("Impulse Sources")] 
    public CinemachineImpulseSource golpeLigeroImpulse;
    public CinemachineImpulseSource golpeFuerteImpulse;
    public CinemachineImpulseSource explosionImpulse;

    [Header("Fuerza de shakes suaves")]
    [SerializeField, Range(0f, 1f)] private float fuerzaBloqueo = 0.6f;
    [SerializeField, Range(0f, 1f)] private float fuerzaImpactoFlecha = 0.5f;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ShakeGolpeLigero()
    {
        golpeLigeroImpulse.GenerateImpulse();
    }

    public void ShakeGolpeFuerte()
    {
        golpeFuerteImpulse.GenerateImpulse();
    }

    public void ShakeExplosion()
    {
        explosionImpulse.GenerateImpulse();
    }
    public void ShakeBloqueo()
    {
        golpeLigeroImpulse.GenerateImpulseWithForce(fuerzaBloqueo);
    }

    public void ShakeImpactoFlecha()
    {
        golpeLigeroImpulse.GenerateImpulseWithForce(fuerzaImpactoFlecha);
    }
}

