using UnityEngine;

[CreateAssetMenu(fileName = "Nueva Habilidad", menuName = "Habilidades/Habilidad")]
public class HabilidadData : ScriptableObject
{
    public string id; 
    public string nombre;
    [TextArea] public string descripcion;

    public Sprite icono;
    public Sprite gifPreview;

    public HabilidadData requisito;
    public int costo;
}

