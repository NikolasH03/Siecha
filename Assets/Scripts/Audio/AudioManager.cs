using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Reproduce un SFX en un punto del mundo con variación de pitch y volumen
    public void PlaySFX(SoundData data, Vector3 position)
    {
        GameObject tempGO = new GameObject("TempAudio_" + data.name);
        tempGO.transform.position = position;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = data.clip;

        float pitch = data.pitch + Random.Range(-data.randomPitchRange, data.randomPitchRange);
        float volume = data.volume + Random.Range(-data.randomVolumeRange, data.randomVolumeRange);

        audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.spatialBlend = 1f; 
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;

        audioSource.Play();
        Destroy(tempGO, data.clip.length / audioSource.pitch + 0.1f);
    }

    // Reproduce música de fondo
    public void PlayMusic(SoundData data)
    {
        if (musicSource == null) return;

        musicSource.clip = data.clip;
        musicSource.volume = data.volume;
        musicSource.pitch = data.pitch;
        musicSource.loop = data.loop;
        musicSource.Play();
    }

    // Reproduce pasos en bucle (por movimiento, no para Animation Events)

    ////public void PlayFootstep(bool isSprinting, SoundData walkClip, SoundData runClip)
    ////{
    ////    if (footstepsSource == null) return;

    ////    SoundData selected = isSprinting ? runClip : walkClip;

    ////    if (!footstepsSource.isPlaying || footstepsSource.clip != selected.clip)
    ////    {
    ////        footstepsSource.clip = selected.clip;
    ////        footstepsSource.volume = selected.volume;
    ////        footstepsSource.pitch = selected.pitch;
    ////        footstepsSource.loop = true;
    ////        footstepsSource.Play();
    ////    }
    ////}

    ////public void StopFootstep()
    ////{
    ////    if (footstepsSource != null)
    ////        footstepsSource.Stop();
    ////}
}

