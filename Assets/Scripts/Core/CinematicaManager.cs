using System;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Singleton que maneja TODAS las cinemáticas InGame del proyecto.
///
/// USO:
///   CinematicaManager.Instance.Reproducir(miDirector, () => Debug.Log("terminó"));
///
/// MODO PLACEHOLDER:
///   Si el director es null, el callback se llama inmediatamente.
///   Sirve para trabajar sin que arte haya entregado los clips.
///
/// INTEGRACIÓN CON INPUT:
///   No toca InputJugador directamente. Avisa a MenuManager para que
///   él gestione el bloqueo de input, igual que ya hace con los menús.
/// </summary>
public class CinematicaManager : MonoBehaviour
{
    public static CinematicaManager Instance { get; private set; }

    private PlayableDirector directorActivo;
    private Action           callbackActivo;
    private bool             enCinematica = false;

    // ─── Eventos ──────────────────────────────────────────────────────────────
    // Cualquier sistema puede suscribirse si necesita reaccionar.
    // CameraManager, AudioManager, etc. pueden escuchar sin acoplarse.
    public static event Action OnCinematicaInicia;
    public static event Action OnCinematicaTermina;

    // ─── Singleton ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Reproduce una cinemática. Si director es null, actúa como placeholder:
    /// llama onTerminada inmediatamente sin bloquear gameplay.
    /// </summary>
    public void Reproducir(PlayableDirector director, Action onTerminada)
    {
        if (director == null)
        {
            Debug.Log("[CinematicaManager] Placeholder — sin director asignado, continuando.");
            onTerminada?.Invoke();
            return;
        }

        if (enCinematica)
        {
            Debug.LogWarning("[CinematicaManager] Ya hay una cinemática en curso. Ignorando solicitud.");
            return;
        }

        callbackActivo = onTerminada;
        directorActivo = director;
        enCinematica   = true;

        MenuManager.Instance?.IniciarModoCinematica();
        OnCinematicaInicia?.Invoke();

        directorActivo.gameObject.SetActive(true);
        directorActivo.stopped += OnDirectorTerminado;
        directorActivo.Play();
    }

    /// <summary>
    /// Fuerza el fin de la cinemática activa (cambio de escena, muerte del jugador, etc.)
    /// </summary>
    public void ForzarDetener()
    {
        if (!enCinematica || directorActivo == null) return;

        directorActivo.stopped -= OnDirectorTerminado;
        directorActivo.Stop();
        var cam = GameObject.Find("camara")
            ?.GetComponent<Camera>();

        if (cam != null)
            cam.enabled = false;
        directorActivo.gameObject.SetActive(false);
        directorActivo = null;

        FinalizarCinematica();
        callbackActivo = null;
    }

    public bool EnCinematica => enCinematica;

    // ─── Callbacks ────────────────────────────────────────────────────────────

    private void OnDirectorTerminado(PlayableDirector director)
    {
        
        director.gameObject.SetActive(false);
        directorActivo = null;
        ForzarDetener();
        FinalizarCinematica();

        director.stopped -= OnDirectorTerminado;
        // Callback DESPUÉS de reanudar para que el estado ya tenga input activo
        var cb = callbackActivo;
        callbackActivo = null;
        cb?.Invoke();
    }

    private void FinalizarCinematica()
    {
        enCinematica = false;
        MenuManager.Instance?.TerminarModoCinematica();
        OnCinematicaTermina?.Invoke();
    }
}
