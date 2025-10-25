using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Audio System")]
    public AudioMixer mixer;

    [Header("Sliders")]
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderAmbience;
    public Slider sliderSFX;
    public Slider sliderVoices;

    [Header("Configuración")]
    [SerializeField] private bool cargarEnAwake = true;
    [SerializeField] private bool guardarAutomaticamente = false; 

    // Claves para PlayerPrefs
    private const string MASTER_KEY = "VolumenMaster";
    private const string MUSIC_KEY = "VolumenMusic";
    private const string AMBIENCE_KEY = "VolumenAmbience";
    private const string SFX_KEY = "VolumenSFX";
    private const string VOICES_KEY = "VolumenVoices";

    private void Awake()
    {
        // Configurar valores por defecto
        ConfigurarValoresPorDefecto();

        if (cargarEnAwake)
        {
            CargarConfiguracion();
        }
    }

    private void Start()
    {
        // Suscribirse a eventos de sliders si guardar automáticamente está activado
        if (guardarAutomaticamente)
        {
            if (sliderMaster) sliderMaster.onValueChanged.AddListener(delegate { GuardarVolumenMaster(); });
            if (sliderMusic) sliderMusic.onValueChanged.AddListener(delegate { GuardarVolumenMusic(); });
            if (sliderAmbience) sliderAmbience.onValueChanged.AddListener(delegate { GuardarVolumenAmbience(); });
            if (sliderSFX) sliderSFX.onValueChanged.AddListener(delegate { GuardarVolumenSFX(); });
            if (sliderVoices) sliderVoices.onValueChanged.AddListener(delegate { GuardarVolumenVoices(); });
        }
    }

    private void ConfigurarValoresPorDefecto()
    {
        // Solo configurar si no existe la configuración
        if (!PlayerPrefs.HasKey(MASTER_KEY))
        {
            if (sliderMaster) sliderMaster.value = 1f;
            PlayerPrefs.SetFloat(MASTER_KEY, 1f);
        }

        if (!PlayerPrefs.HasKey(MUSIC_KEY))
        {
            if (sliderMusic) sliderMusic.value = 1f;
            PlayerPrefs.SetFloat(MUSIC_KEY, 1f);
        }

        if (!PlayerPrefs.HasKey(AMBIENCE_KEY))
        {
            if (sliderAmbience) sliderAmbience.value = 1f;
            PlayerPrefs.SetFloat(AMBIENCE_KEY, 1f);
        }

        if (!PlayerPrefs.HasKey(SFX_KEY))
        {
            if (sliderSFX) sliderSFX.value = 1f;
            PlayerPrefs.SetFloat(SFX_KEY, 1f);
        }

        if (!PlayerPrefs.HasKey(VOICES_KEY))
        {
            if (sliderVoices) sliderVoices.value = 1f;
            PlayerPrefs.SetFloat(VOICES_KEY, 1f);
        }
    }

    // ========== CARGAR CONFIGURACIÓN ==========
    public void CargarConfiguracion()
    {
        // Cargar valores desde PlayerPrefs y aplicar a sliders y AudioMixer
        float masterVol = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float ambienceVol = PlayerPrefs.GetFloat(AMBIENCE_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        float voicesVol = PlayerPrefs.GetFloat(VOICES_KEY, 1f);

        // Aplicar a sliders (sin disparar eventos)
        if (sliderMaster)
        {
            sliderMaster.SetValueWithoutNotify(masterVol);
        }
        if (sliderMusic)
        {
            sliderMusic.SetValueWithoutNotify(musicVol);
        }
        if (sliderAmbience)
        {
            sliderAmbience.SetValueWithoutNotify(ambienceVol);
        }
        if (sliderSFX)
        {
            sliderSFX.SetValueWithoutNotify(sfxVol);
        }
        if (sliderVoices)
        {
            sliderVoices.SetValueWithoutNotify(voicesVol);
        }

        // Aplicar al AudioMixer
        AplicarVolumenCompleto();

        Debug.Log("Configuración de volumen cargada desde PlayerPrefs");
    }

    // ========== GUARDAR CONFIGURACIÓN ==========
    public void GuardarConfiguracion()
    {
        if (sliderMaster) PlayerPrefs.SetFloat(MASTER_KEY, sliderMaster.value);
        if (sliderMusic) PlayerPrefs.SetFloat(MUSIC_KEY, sliderMusic.value);
        if (sliderAmbience) PlayerPrefs.SetFloat(AMBIENCE_KEY, sliderAmbience.value);
        if (sliderSFX) PlayerPrefs.SetFloat(SFX_KEY, sliderSFX.value);
        if (sliderVoices) PlayerPrefs.SetFloat(VOICES_KEY, sliderVoices.value);

        PlayerPrefs.Save();
        Debug.Log("Configuración de volumen guardada en PlayerPrefs");
    }

    // ========== GUARDAR INDIVIDUALES ==========
    public void GuardarVolumenMaster()
    {
        if (sliderMaster)
        {
            PlayerPrefs.SetFloat(MASTER_KEY, sliderMaster.value);
            if (guardarAutomaticamente) PlayerPrefs.Save();
        }
    }

    public void GuardarVolumenMusic()
    {
        if (sliderMusic)
        {
            PlayerPrefs.SetFloat(MUSIC_KEY, sliderMusic.value);
            if (guardarAutomaticamente) PlayerPrefs.Save();
        }
    }

    public void GuardarVolumenAmbience()
    {
        if (sliderAmbience)
        {
            PlayerPrefs.SetFloat(AMBIENCE_KEY, sliderAmbience.value);
            if (guardarAutomaticamente) PlayerPrefs.Save();
        }
    }

    public void GuardarVolumenSFX()
    {
        if (sliderSFX)
        {
            PlayerPrefs.SetFloat(SFX_KEY, sliderSFX.value);
            if (guardarAutomaticamente) PlayerPrefs.Save();
        }
    }

    public void GuardarVolumenVoices()
    {
        if (sliderVoices)
        {
            PlayerPrefs.SetFloat(VOICES_KEY, sliderVoices.value);
            if (guardarAutomaticamente) PlayerPrefs.Save();
        }
    }

    // ========== APLICAR VOLUMEN ==========
    public void AplicarVolumen()
    {
        AplicarVolumenCompleto();
    }

    private void AplicarVolumenCompleto()
    {
        if (mixer == null) return;

        if (sliderMaster)
            mixer.SetFloat("Master", ConvertirASonido(sliderMaster.value));
        if (sliderMusic)
            mixer.SetFloat("Music", ConvertirASonido(sliderMusic.value));
        if (sliderAmbience)
            mixer.SetFloat("Ambience", ConvertirASonido(sliderAmbience.value));
        if (sliderSFX)
            mixer.SetFloat("SFX", ConvertirASonido(sliderSFX.value));
        if (sliderVoices)
            mixer.SetFloat("Voices", ConvertirASonido(sliderVoices.value));
    }

    private float ConvertirASonido(float valorSlider)
    {
        return Mathf.Log10(Mathf.Clamp(valorSlider, 0.0001f, 1f)) * 20f;
    }

    // ========== MÉTODOS DE UTILIDAD ==========
    public void ResetearADefecto()
    {
        if (sliderMaster) sliderMaster.value = 1f;
        if (sliderMusic) sliderMusic.value = 1f;
        if (sliderAmbience) sliderAmbience.value = 1f;
        if (sliderSFX) sliderSFX.value = 1f;
        if (sliderVoices) sliderVoices.value = 1f;

        AplicarVolumenCompleto();
        GuardarConfiguracion();
    }

    // Getters para acceder a los valores actuales
    public float VolumenMaster => sliderMaster ? sliderMaster.value : PlayerPrefs.GetFloat(MASTER_KEY, 1f);
    public float VolumenMusic => sliderMusic ? sliderMusic.value : PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
    public float VolumenAmbience => sliderAmbience ? sliderAmbience.value : PlayerPrefs.GetFloat(AMBIENCE_KEY, 1f);
    public float VolumenSFX => sliderSFX ? sliderSFX.value : PlayerPrefs.GetFloat(SFX_KEY, 1f);
    public float VolumenVoices => sliderVoices ? sliderVoices.value : PlayerPrefs.GetFloat(VOICES_KEY, 1f);
}
