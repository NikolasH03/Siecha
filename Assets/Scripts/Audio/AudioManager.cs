using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manager global de audio. Persiste entre escenas (DontDestroyOnLoad).
/// 
/// ARQUITECTURA:
///   - SFX espaciales  → PlaySFX / PlayRandomSFX  (usan Object Pool, sin GC spikes)
///   - Música          → PlayMusic / StopMusic / FadeMusic
///   - Ambience        → PlayAmbience
///   - Voces           → PlayVoiceLine
///
/// CÓMO AGREGAR SONIDOS:
///   En lugar de agregar campos aquí, crea un asset SoundLibrary en el proyecto
///   (Audio > Sound Library) y referencíalo donde lo necesites.
///   Ejemplo: SoundLibrary_UI tiene "ui_hover", "ui_confirm", "ui_pause".
///   Uso: AudioManager.Instance.PlaySFX(uiSounds.Get("ui_hover"), transform.position);
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ─── Referencias de Inspector ───────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource voicesSource;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Librerías de Sonido Global")]
    [Tooltip("Sonidos de UI: hover, confirm, pause, pickup, etc.")]
    [SerializeField] private SoundLibrary uiSounds;
    [Tooltip("Música: menu, combate, boss, muerte, etc.")]
    [SerializeField] private SoundLibrary musicSounds;
    [Tooltip("Ambiente: bosque, aldea, cueva, etc.")]
    [SerializeField] private SoundLibrary ambientSounds;

    // ─── Object Pool ─────────────────────────────────────────────────────────
    // En lugar de crear y destruir un GameObject por sonido (lo que genera
    // Garbage Collection spikes en combate), mantenemos una cola de AudioSources
    // reutilizables. Cuando se necesita uno, se saca de la cola; cuando termina,
    // vuelve a la cola. Nunca se destruyen, solo se desactivan.
    [Header("Object Pool")]
    [Tooltip("Número de AudioSources pre-creados al inicio. Ajusta según la cantidad " +
             "de sonidos simultáneos esperados (hordas de enemigos, efectos de combate).")]
    [SerializeField] private int poolSize = 20;
    private Queue<AudioSource> _pool;

    // ─── Singleton ───────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ─── Fade ────────────────────────────────────────────────────────────────
    private Coroutine _fadeCoroutine;

    // ════════════════════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        transform.parent = null;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitPool();
    }

    /// <summary>
    /// Pre-crea los AudioSources del pool para evitar allocations en runtime.
    /// </summary>
    private void InitPool()
    {
        _pool = new Queue<AudioSource>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            _pool.Enqueue(CreatePooledSource());
        }
    }

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform); // Hijo del AudioManager para que no ensucien la jerarquía
        DontDestroyOnLoad(go);

        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 20f;

        go.SetActive(false);
        return source;
    }

    private AudioSource GetFromPool()
    {
        // Si el pool está vacío (más sonidos simultáneos que poolSize), crea uno extra.
        // Esto evita que el juego falle; considera aumentar poolSize si esto ocurre seguido.
        if (_pool.Count == 0)
        {
            Debug.LogWarning("[AudioManager] Pool vacío: creando AudioSource extra. " +
                             "Considera aumentar poolSize en el Inspector.");
            return CreatePooledSource();
        }

        AudioSource source = _pool.Dequeue();
        source.gameObject.SetActive(true);
        return source;
    }

    private void ReturnToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        _pool.Enqueue(source);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SFX ESPACIALES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reproduce un SFX en una posición del mundo con variación de pitch y volumen.
    /// Usa el Object Pool: no crea ni destruye GameObjects, sin GC spikes.
    /// </summary>
    public void PlaySFX(SoundData data, Vector3 position)
    {
        // Validación defensiva: si alguien olvida asignar un SoundData en el Inspector,
        // el juego no se cae — solo loguea una advertencia clara.
        if (data == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX: SoundData es null.");
            return;
        }
        if (data.clip == null)
        {
            Debug.LogWarning($"[AudioManager] PlaySFX: '{data.name}' no tiene AudioClip asignado.");
            return;
        }

        AudioSource source = GetFromPool();
        source.transform.position = position;
        source.clip = data.clip;

        float pitch  = data.pitch  + Random.Range(-data.randomPitchRange,  data.randomPitchRange);
        float volume = data.volume + Random.Range(-data.randomVolumeRange, data.randomVolumeRange);

        source.pitch  = Mathf.Clamp(pitch,  0.1f, 3f);
        source.volume = Mathf.Clamp01(volume);
        source.loop   = false;

        source.Play();

        // Devuelve al pool cuando el clip termina.
        // Dividir por pitch es importante: si el pitch es 2.0, el sonido dura la mitad.
        StartCoroutine(ReturnToPoolAfterPlay(source, data.clip.length / source.pitch));
    }

    /// <summary>
    /// Elige un sonido aleatorio del RandomSoundSet y lo reproduce en la posición dada.
    /// </summary>
    public void PlayRandomSFX(RandomSoundSet soundSet, Vector3 position)
    {
        if (soundSet == null)
        {
            Debug.LogWarning("[AudioManager] PlayRandomSFX: RandomSoundSet es null.");
            return;
        }

        SoundData randomSound = soundSet.GetRandomSound();
        if (randomSound != null)
            PlaySFX(randomSound, position);
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.05f); // pequeño margen de seguridad
        ReturnToPool(source);
    }

    // ════════════════════════════════════════════════════════════════════════
    // MÉTODOS DE CONVENIENCIA PARA LIBRERÍAS GLOBALES
    // Permiten reproducir sonidos de UI, música y ambiente desde cualquier
    // parte del código con solo el ID, sin necesidad de referenciar la librería.
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Reproduce un sonido de UI por ID. Posición: Vector3.zero (2D, sin espacialidad).</summary>
    public void PlayUI(string soundId)
    {
        if (uiSounds == null) return;
        SoundData data = uiSounds.Get(soundId);
        if (data != null) PlaySFX(data, Vector3.zero);
    }

    /// <summary>Inicia la música identificada por ID.</summary>
    public void PlayMusicById(string musicId)
    {
        if (musicSounds == null) return;
        SoundData data = musicSounds.Get(musicId);
        if (data != null) PlayMusic(data);
    }

    /// <summary>Inicia el ambiente identificado por ID.</summary>
    public void PlayAmbienceById(string ambienceId)
    {
        if (ambientSounds == null) return;
        SoundData data = ambientSounds.Get(ambienceId);
        if (data != null) PlayAmbience(data);
    }

    // ════════════════════════════════════════════════════════════════════════
    // MÚSICA
    // ════════════════════════════════════════════════════════════════════════

    public void PlayMusic(SoundData data)
    {
        if (musicSource == null || data == null || data.clip == null) return;

        musicSource.clip   = data.clip;
        musicSource.volume = data.volume;
        musicSource.pitch  = data.pitch;
        musicSource.loop   = data.loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    /// <summary>
    /// Hace fade out de la música actual y fade in de la nueva.
    /// Útil para transiciones entre zonas o estados del juego.
    /// </summary>
    public void FadeToMusic(SoundData data, float fadeDuration = 1f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeMusicCoroutine(data, fadeDuration));
    }

    private IEnumerator FadeMusicCoroutine(SoundData newMusic, float duration)
    {
        float startVolume = musicSource.volume;

        // Fade out
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.Stop();
        if (newMusic != null && newMusic.clip != null)
        {
            musicSource.clip = newMusic.clip;
            musicSource.loop = newMusic.loop;
            musicSource.pitch = newMusic.pitch;
            musicSource.Play();

            // Fade in
            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, newMusic.volume, t / duration);
                yield return null;
            }
            musicSource.volume = newMusic.volume;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // AMBIENTE Y VOCES
    // ════════════════════════════════════════════════════════════════════════

    public void PlayAmbience(SoundData data)
    {
        if (ambienceSource == null || data == null || data.clip == null) return;

        ambienceSource.clip   = data.clip;
        ambienceSource.volume = data.volume;
        ambienceSource.pitch  = data.pitch;
        ambienceSource.loop   = data.loop;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource == null) return;
        ambienceSource.Stop();
    }

    /// <summary>
    /// Reproduce una línea de voz. Solo puede sonar una a la vez (voicesSource).
    /// Para múltiples personajes hablando simultáneamente necesitarías múltiples AudioSources de voz.
    /// </summary>
    public void PlayVoiceLine(SoundData data)
    {
        if (voicesSource == null || data == null || data.clip == null) return;

        voicesSource.clip   = data.clip;
        voicesSource.volume = data.volume;
        voicesSource.pitch  = data.pitch;
        voicesSource.loop   = data.loop;
        voicesSource.Play();
    }
}

