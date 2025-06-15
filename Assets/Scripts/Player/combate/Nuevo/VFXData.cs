using UnityEngine;

[CreateAssetMenu(menuName = "VFX/VFXData")]
public class VFXData : ScriptableObject
{
    public GameObject prefab;
    public float lifetime = 1f;
}
