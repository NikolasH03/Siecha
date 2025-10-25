using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CinematicPlayer : MonoBehaviour
{
    [SerializeField] private GameObject Container;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage cinematicImage;
    [SerializeField] private GameObject skipPromptUI;

    private RenderTexture renderTexture;
    private bool skipPromptActive = false;
    private float skipPromptTimer = 2f;
    private bool isPlaying = false;

    public System.Action OnCinematicFinished;


    public void Awake()
    {
        Container.SetActive(false);
        skipPromptUI.SetActive(false);
    }
    public void PlayCinematic(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("No hay clip de video asignado.");
            return;
        }

        //limpieza de seguridad
        CancelInvoke();
        skipPromptActive = false;
        skipPromptUI.SetActive(false);

        //Pausar el juego
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Crear RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();

        // Configurar VideoPlayer
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();

        // Asignar al RawImage
        cinematicImage.texture = renderTexture;
        Container.SetActive(true);

        isPlaying = true;
        Debug.Log($"Reproduciendo cinemática: {clip.name}");

    }


    private void Update()
    {
        if (!isPlaying) return;

        // Salto de cinemática
        if (InputJugador.instance.Interactuar)
        {
            if (!skipPromptActive)
            {
                skipPromptActive = true;
                skipPromptUI.SetActive(true);
            }
            else
            {
                StopCinematic();
            }
        }

        if (skipPromptActive)
        {
            skipPromptTimer -= Time.unscaledDeltaTime;
            if (skipPromptTimer <= 0f)
            {
                skipPromptActive = false;
                skipPromptUI.SetActive(false);
                skipPromptTimer = 2f;
            }
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StopCinematic();
    }

    public void StopCinematic()
    {
        // Evitar llamadas dobles
        if (!isPlaying) return;

        isPlaying = false;

        // Cancelar invocaciones pendientes (como EnableSkip)
        CancelInvoke();

        // Quitar evento del video
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.Stop();
        videoPlayer.targetTexture = null;

        // Liberar render texture
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        // Reset visuales
        cinematicImage.texture = null;
        skipPromptUI.SetActive(false);
        Container.SetActive(false);

        // Restaurar juego
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Reset flags completamente
        skipPromptActive = false;
        skipPromptTimer = 2f;

        OnCinematicFinished?.Invoke();
        Debug.Log("Cinemática terminada y reseteada correctamente.");
    }

}
