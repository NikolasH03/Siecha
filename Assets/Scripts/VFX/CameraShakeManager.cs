using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    [Header("Impulse Sources")] 
    public CinemachineImpulseSource golpeLigeroImpulse;
    public CinemachineImpulseSource golpeFuerteImpulse;
    public CinemachineImpulseSource explosionImpulse;

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
}

