using UnityEngine;
using TMPro;
public class UIVisualizador3D : MonoBehaviour
{
    public static UIVisualizador3D instance;
    [SerializeField] TextMeshProUGUI textoNombre;
    [SerializeField] TextMeshProUGUI textoDescripcion;
    [SerializeField] Transform contenedorModelo3D;
    [SerializeField] GameObject contenedor;

    public float velocidadRotacion = 50f;


    private GameObject instanciaActual;

    private void Awake()
    {
        instance = this;

        contenedor.SetActive(false);
    }
    public void RotarModelo(Vector2 entrada)
    {
        Vector3 rotacion = new Vector3(-entrada.y, entrada.x, 0f) * velocidadRotacion * Time.unscaledDeltaTime;
        contenedorModelo3D.Rotate(rotacion, Space.Self);
    }
    public void Mostrar(ColeccionableData data)
    {
        if (instanciaActual != null)
            Destroy(instanciaActual);

        instanciaActual = Instantiate(data.modelo3D, contenedorModelo3D);
        instanciaActual.transform.localPosition = Vector3.zero;
        instanciaActual.transform.localRotation = Quaternion.identity;
        instanciaActual.transform.localScale *= 10;

        instanciaActual.layer = LayerMask.NameToLayer("Coleccionable");
        foreach (Transform t in instanciaActual.GetComponentsInChildren<Transform>())
            t.gameObject.layer = LayerMask.NameToLayer("Coleccionable");

        textoNombre.text = data.nombre;
        textoDescripcion.text = data.descripcion;

        contenedor.SetActive(true);
    }

    public void Cerrar()
    {
        if (instanciaActual != null)
            Destroy(instanciaActual);
        contenedor.SetActive(false);
    }
}
