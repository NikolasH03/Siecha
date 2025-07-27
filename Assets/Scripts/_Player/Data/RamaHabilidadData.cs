using UnityEngine;

[CreateAssetMenu(fileName = "Nueva Rama", menuName = "Habilidades/Rama")]
public class RamaHabilidadData : ScriptableObject
{
    public string nombreRama;
    public Sprite icono;
    public HabilidadData[] habilidades;
}

