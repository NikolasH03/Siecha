using UnityEngine;
using UnityEngine.UI;

public class MinijuegoRecargaUI : MonoBehaviour
{
    public Image barraFondo;
    public RectTransform marcador;
    public RectTransform zonaPerfecta;

    public float velocidad = 300f;
    public bool recargaPerfecta = false;

    private bool activo = false;
    private float ancho;
    private float posInicial;
    private float tiempoTotal = 2f;
    private float tiempoActual = 0f;

    public System.Action<bool> OnFinRecarga; 

    void OnEnable()
    {
        ancho = barraFondo.rectTransform.rect.width;
        posInicial = -ancho / 2f;
        tiempoActual = 0f;
        marcador.anchoredPosition = new Vector2(posInicial, marcador.anchoredPosition.y);
        recargaPerfecta = false;
        activo = true;
    }

    void Update()
    {
        if (!activo) return;

        tiempoActual += Time.deltaTime;
        float x = Mathf.Lerp(posInicial, -posInicial, tiempoActual / tiempoTotal);
        marcador.anchoredPosition = new Vector2(x, marcador.anchoredPosition.y);

        if (InputJugador.instance.recargar)
        {
            float marcadorX = marcador.anchoredPosition.x;
            float izquierda = zonaPerfecta.anchoredPosition.x - (zonaPerfecta.rect.width / 2f);
            float derecha = zonaPerfecta.anchoredPosition.x + (zonaPerfecta.rect.width / 2f);

            if (marcadorX >= izquierda && marcadorX <= derecha)
            {
                recargaPerfecta = true;
                Debug.Log("¡Recarga perfecta!");
            }
            else
            {
                Debug.Log("¡Fallaste la recarga!");
            }

            activo = false;
            OnFinRecarga?.Invoke(recargaPerfecta);
            gameObject.SetActive(false);
        }

        if (tiempoActual >= tiempoTotal)
        {
            Debug.Log("Tiempo agotado");
            activo = false;
            OnFinRecarga?.Invoke(false);
            gameObject.SetActive(false);
        }
    }
}

