using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuTotem : MenuBaseConNavegacion
{
    [Header("UI Panel Mejoras")]
    [SerializeField] private Button botonMejora1;
    [SerializeField] private Button botonMejora2;

    // Controlador logica de totem
    TotemMejora totem;

    private void Awake()
    {
        pauseGame = true; // Pausar el juego cuando aparece
    }

    private void Start() { 
    
        if(MenuManager.Instance.EstaEnGameplay)
        {
            totem = GameObject.FindGameObjectWithTag("Totem").GetComponent<TotemMejora>();
        }

        // Configurar botones
        if (botonMejora1) botonMejora1.onClick.AddListener(ElegirMejora1);
        if (botonMejora2) botonMejora2.onClick.AddListener(ElegirMejora2);
    }

    protected override void ConfigurarNavegacion()
    {
        if (botonMejora1 && botonMejora2)
        {
            ConfigurarNavegacionBoton(botonMejora1, derecha: botonMejora2);
            ConfigurarNavegacionBoton(botonMejora2, izquierda: botonMejora1);

            primerSeleccionable = botonMejora1;
        }
    }

    public void ElegirMejora1()
    {
        totem.AumentarVida();
        CerrarPanel();
    }

    public void ElegirMejora2()
    {
        totem.AumentarEstamina();
        CerrarPanel();
    }

    private void CerrarPanel()
    {
        MenuManager.Instance?.GoBack();
        Debug.Log("Panel de mejoras cerrado");
    }

    private void OnDestroy()
    {
        if (botonMejora1) botonMejora1.onClick.RemoveListener(ElegirMejora1);
        if (botonMejora2) botonMejora2.onClick.RemoveListener(ElegirMejora2);
    }
}