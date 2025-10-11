using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderAmbience;
    public Slider sliderSFX;
    public Slider sliderVoices;

    private void Awake()
    {
        sliderMaster.value = 1f;
        sliderMusic.value = 1f;
        sliderAmbience.value = 1f;
        sliderSFX.value = 1f;
        sliderVoices.value = 1f;

        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        AplicarVolumen();
    }

    public void AplicarVolumen()
    {
        mixer.SetFloat("Master", ConvertirASonido(sliderMaster.value));
        mixer.SetFloat("Music", ConvertirASonido(sliderMusic.value));
        mixer.SetFloat("Ambience", ConvertirASonido(sliderAmbience.value));
        mixer.SetFloat("SFX", ConvertirASonido(sliderSFX.value));
        mixer.SetFloat("Voices", ConvertirASonido(sliderVoices.value));
    }

    float ConvertirASonido(float valorSlider)
    {
        return Mathf.Log10(Mathf.Clamp(valorSlider, 0.0001f, 1f)) * 20f;
    }
}
