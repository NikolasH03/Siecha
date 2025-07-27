using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VisualizadorNodoUI : MonoBehaviour
{
    public static VisualizadorNodoUI instance;

    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;
    public TextMeshProUGUI textoCosto;
    public Image imagenGif;
    public GameObject panelVisualizador;

    private void Awake()
    {
        instance = this;
    }

    public void Mostrar(HabilidadData data)
    {
        textoNombre.text = data.nombre;
        textoDescripcion.text = data.descripcion;
        textoCosto.text = "Costo: " + data.costo.ToString();
        imagenGif.sprite = data.gifPreview;  
        panelVisualizador.SetActive(true);
    }

    public void Ocultar()
    {
        panelVisualizador.SetActive(false);
    }
}

