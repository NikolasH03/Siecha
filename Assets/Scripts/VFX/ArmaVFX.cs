using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmaVFX : MonoBehaviour
{
    [SerializeField] GameObject trail;
    public void ActivarTrail() => trail.SetActive(true);
    public void DesactivarTrail() => trail.SetActive(false);

}
