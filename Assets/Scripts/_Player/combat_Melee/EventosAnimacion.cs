using System.Collections.Generic;
using UnityEngine;

public class EventosAnimacion : MonoBehaviour
{
    [Header("SFX del personaje")]
    [SerializeField] private List<SoundData> sonidos;

    [Header("VFX del personaje")]
    [SerializeField] private List<VFXData> efectos;

    [Header("Puntos de impacto")]
    [SerializeField] private List<Transform> pivotsImpacto;



    // Reproduce un sonido desde un punto de impacto específico.
    public void ReproducirSonidoImpacto(int indexSonido, int indexPivot = 0)
    {
        if (indexSonido >= 0 && indexSonido < sonidos.Count)
        {
            Vector3 posicion = ObtenerPosicionPivot(indexPivot);
            AudioManager.Instance.PlaySFX(sonidos[indexSonido], posicion);
        }
        else
        {
            Debug.LogWarning($"{name}: índice de sonido fuera de rango.");
        }
    }


    // Reproduce un VFX desde un punto de impacto específico.
    public void ReproducirVFX(int indexVFX, int indexPivot = 0)
    {
        if (indexVFX >= 0 && indexVFX < efectos.Count)
        {
            Vector3 posicion = ObtenerPosicionPivot(indexPivot);
            Quaternion rotacion = ObtenerRotacionPivot(indexPivot);
            VFXPool.Instance.PlayVFX(efectos[indexVFX], posicion, rotacion);
        }
        else
        {
            Debug.LogWarning($"{name}: índice de VFX fuera de rango.");
        }
    }


    // Retorna la posición del pivot solicitado, o la del objeto si no hay pivots.
    private Vector3 ObtenerPosicionPivot(int index)
    {
        if (index >= 0 && index < pivotsImpacto.Count && pivotsImpacto[index] != null)
        {
            return pivotsImpacto[index].position;
        }

        Debug.LogWarning($"{name}: Pivot de impacto no encontrado, usando posición por defecto.");
        return transform.position;
    }


    // Retorna la rotación del pivot solicitado, o la del objeto si no hay pivots.
    private Quaternion ObtenerRotacionPivot(int index)
    {
        if (index >= 0 && index < pivotsImpacto.Count && pivotsImpacto[index] != null)
        {
            return pivotsImpacto[index].rotation;
        }

        return transform.rotation;
    }
}


