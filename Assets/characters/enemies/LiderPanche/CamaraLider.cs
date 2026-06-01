using UnityEngine;

public class CamaraLider : MonoBehaviour
{
    [SerializeField] private Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;
        transform.rotation = target.rotation;
        // No copiamos la escala
    }
}