using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuGraficos : MenuBaseConNavegacion
{
    [Header("Opciones de Video")]
    [SerializeField] private Toggle togglePantallaCompleta;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonVolver;

    [Header("Botones BarraOpciones")]
    [SerializeField] private Button botonControles;
    [SerializeField] private Button botonVolumen;
    [SerializeField] private Button botonGraficas;

    private const string PREF_PANTALLA_COMPLETA = "PantallaCompleta";
    private bool pantallaCompletaActual;
    protected override void OnMenuOpened()
    {
        pantallaCompletaActual = PlayerPrefs.GetInt(PREF_PANTALLA_COMPLETA, 1) == 1;

        Screen.fullScreen = pantallaCompletaActual;
        if (togglePantallaCompleta)
            togglePantallaCompleta.isOn = pantallaCompletaActual;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (togglePantallaCompleta)
            togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);
    }
    public void CambiarPantallaCompleta(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
    }
    protected override void ConfigurarNavegacion()
    {
        if (botonControles && botonVolumen && botonGraficas && togglePantallaCompleta && botonGuardar && botonVolver)
        {
            ConfigurarNavegacionBoton(botonControles,
               arriba: botonGuardar, abajo: togglePantallaCompleta, derecha: botonVolumen, izquierda: botonGraficas);
            ConfigurarNavegacionBoton(botonVolumen,
                arriba: botonGuardar, abajo: togglePantallaCompleta, derecha: botonGraficas, izquierda: botonControles);
            ConfigurarNavegacionBoton(botonGraficas,
                arriba: botonGuardar, abajo: togglePantallaCompleta, derecha: botonControles, izquierda: botonVolumen);
            ConfigurarNavegacionBoton(togglePantallaCompleta,
                arriba: botonGraficas, abajo: botonGuardar);
            ConfigurarNavegacionBoton(botonGuardar,
                abajo: botonGraficas, arriba: togglePantallaCompleta, derecha: botonVolver);
            ConfigurarNavegacionBoton(botonVolver,
                abajo: botonGraficas, arriba: togglePantallaCompleta, izquierda: botonGuardar);

            primerSeleccionable = botonGraficas;
        }

    }

    // --- Métodos ---
    public void AbrirVolumen()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuVolumen);
    }

    public void AbrirControles()
    {
        MenuManager.Instance.OpenMenu(MenuManager.Instance.MenuControles);
    }
    public void GuardarYVolver()
    {
        bool pantallaCompleta = togglePantallaCompleta.isOn;
        PlayerPrefs.SetInt(PREF_PANTALLA_COMPLETA, pantallaCompleta ? 1 : 0);
        PlayerPrefs.Save();

        MenuManager.Instance.GoBackToPreviousCoreMenu();
    }

    public void VolverSinGuardar()
    {
        Screen.fullScreen = pantallaCompletaActual;
        MenuManager.Instance.GoBackToPreviousCoreMenu();
    }
}
