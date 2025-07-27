using UnityEngine;

[CreateAssetMenu(fileName = "NuevoColeccionable", menuName = "Coleccionables/Coleccionable")]
public class ColeccionableData : ScriptableObject
{
    public int id;
    public string nombre;
    [TextArea] public string descripcion;
    public GameObject modelo3D;
    public Sprite icono;
}

