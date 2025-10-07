using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuVolumen : MenuBaseConNavegacion
{
    [Header("Sistema de Audio")]
    [SerializeField] private VolumeSettings volumeSettings;

    [Header("Sliders de Volumen")]
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderAmbiente;
    [SerializeField] private Slider sliderSFX;
    [SerializeField] private Slider sliderVoces;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonVolver;

    [Header("Botones BarraOpciones")]
    [SerializeField] private Button botonControles;
    [SerializeField] private Button botonVolumen;
    [SerializeField] private Button botonGraficas;


    // Variables para detectar cambios
    private float valorInicialMaster;
    private float valorInicialMusica;
    private float valorInicialAmbiente;
    private float valorInicialSFX;
    private float valorInicialVoces;

    protected override void OnMenuOpened()
    {
        if (volumeSettings != null)
        {
            // Cargar configuración desde PlayerPrefs
            volumeSettings.CargarConfiguracion();

            // Sincronizar valores con los sliders de este menú
            if (sliderMaster)
            {
                sliderMaster.SetValueWithoutNotify(volumeSettings.sliderMaster.value);
                valorInicialMaster = sliderMaster.value;
            }
            if (sliderMusica)
            {
                sliderMusica.SetValueWithoutNotify(volumeSettings.sliderMusic.value);
                valorInicialMusica = sliderMusica.value;
            }
            if (sliderAmbiente)
            {
                sliderAmbiente.SetValueWithoutNotify(volumeSettings.sliderAmbience.value);
                valorInicialAmbiente = sliderAmbiente.value;
            }
            if (sliderSFX)
            {
                sliderSFX.SetValueWithoutNotify(volumeSettings.sliderSFX.value);
                valorInicialSFX = sliderSFX.value;
            }
            if (sliderVoces)
            {
                sliderVoces.SetValueWithoutNotify(volumeSettings.sliderVoices.value);
                valorInicialVoces = sliderVoces.value;
            }
        }

        Debug.Log("Menú Volumen abierto - Configuración cargada");
    }

    protected override void ConfigurarNavegacion()
    {
        if (botonControles && botonVolumen && botonGraficas && sliderMaster && sliderAmbiente && sliderMusica && sliderSFX && sliderVoces && botonGuardar && botonVolver)
        {
            ConfigurarNavegacionBoton(botonControles,
                arriba: botonGuardar, abajo: sliderMaster, derecha: botonVolumen, izquierda: botonGraficas);
            ConfigurarNavegacionBoton(botonVolumen,
                arriba: botonGuardar, abajo: sliderMaster, derecha: botonGraficas, izquierda: botonControles);
            ConfigurarNavegacionBoton(botonGraficas,
                arriba: botonGuardar, abajo: sliderMaster, derecha: botonControles, izquierda: botonVolumen);
            ConfigurarNavegacionBoton(sliderMaster,
                arriba: botonVolumen, abajo: sliderMusica);
            ConfigurarNavegacionBoton(sliderMusica,
                arriba: sliderMaster, abajo: sliderAmbiente);
            ConfigurarNavegacionBoton(sliderAmbiente,
                arriba: sliderMusica, abajo: sliderVoces);
            ConfigurarNavegacionBoton(sliderVoces,
                abajo: sliderSFX, arriba: sliderAmbiente);
            ConfigurarNavegacionBoton(sliderSFX,
                abajo: botonGuardar, arriba: sliderVoces);
            ConfigurarNavegacionBoton(botonGuardar,
                abajo: botonVolumen, arriba: sliderSFX, derecha: botonVolver);
            ConfigurarNavegacionBoton(botonVolver,
                abajo: botonVolumen, arriba: sliderSFX, izquierda: botonGuardar);

            primerSeleccionable = botonVolumen;
        }
    }

    // ========== MÉTODOS DE CAMBIO DE VOLUMEN ==========
    public void CambiarMaster(float valor)
    {
        if (volumeSettings?.sliderMaster)
        {
            volumeSettings.sliderMaster.value = valor;
            volumeSettings.AplicarVolumen();
        }
    }

    public void CambiarMusica(float valor)
    {
        if (volumeSettings?.sliderMusic)
        {
            volumeSettings.sliderMusic.value = valor;
            volumeSettings.AplicarVolumen();
        }
    }

    public void CambiarAmbiente(float valor)
    {
        if (volumeSettings?.sliderAmbience)
        {
            volumeSettings.sliderAmbience.value = valor;
            volumeSettings.AplicarVolumen();
        }
    }

    public void CambiarSFX(float valor)
    {
        if (volumeSettings?.sliderSFX)
        {
            volumeSettings.sliderSFX.value = valor;
            volumeSettings.AplicarVolumen();
        }
    }

    public void CambiarVoces(float valor)
    {
        if (volumeSettings?.sliderVoices)
        {
            volumeSettings.sliderVoices.value = valor;
            volumeSettings.AplicarVolumen();
        }
    }

    // ========== MÉTODOS DE BOTONES ==========
    public void GuardarYVolver()
    {
        if (volumeSettings != null)
        {
            volumeSettings.GuardarConfiguracion();
            Debug.Log("Configuración de volumen guardada");
        }

        MenuManager.Instance.GoBackToPreviousCoreMenu();
    }

    public void VolverSinGuardar()
    {
        // Restaurar valores originales
        if (volumeSettings != null)
        {
            if (sliderMaster && volumeSettings.sliderMaster)
            {
                volumeSettings.sliderMaster.value = valorInicialMaster;
                sliderMaster.SetValueWithoutNotify(valorInicialMaster);
            }
            if (sliderMusica && volumeSettings.sliderMusic)
            {
                volumeSettings.sliderMusic.value = valorInicialMusica;
                sliderMusica.SetValueWithoutNotify(valorInicialMusica);
            }
            if (sliderAmbiente && volumeSettings.sliderAmbience)
            {
                volumeSettings.sliderAmbience.value = valorInicialAmbiente;
                sliderAmbiente.SetValueWithoutNotify(valorInicialAmbiente);
            }
            if (sliderSFX && volumeSettings.sliderSFX)
            {
                volumeSettings.sliderSFX.value = valorInicialSFX;
                sliderSFX.SetValueWithoutNotify(valorInicialSFX);
            }
            if (sliderVoces && volumeSettings.sliderVoices)
            {
                volumeSettings.sliderVoices.value = valorInicialVoces;
                sliderVoces.SetValueWithoutNotify(valorInicialVoces);
            }

            volumeSettings.AplicarVolumen();
            Debug.Log("Cambios de volumen descartados");
        }

        MenuManager.Instance.GoBackToPreviousCoreMenu();
    }

    public void AbrirControles()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuControles);
    }
    public void AbrirGraficos()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuGraficos);
    }
}
