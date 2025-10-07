using System.Collections.Generic;
using UnityEngine;

public class TotemMejora : MonoBehaviour
{
    [SerializeField] private GameObject canvasInteractuar;
    [SerializeField] private GameObject HUDJugador;
    private bool jugadorCerca = false;
    private bool yaUsoTotem = false;
    private ControladorCombate jugador;

    private void Awake()
    {
        canvasInteractuar.SetActive(false);
    }
    private void Update()
    {
        if (yaUsoTotem) return;

        if (jugadorCerca && InputJugador.instance.Interactuar)
        {
            MenuManager.Instance.MostrarPanelTotem();
            HUDJugador.SetActive(false);
            canvasInteractuar.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaUsoTotem) return;

        if (other.CompareTag("Player"))
        {
            canvasInteractuar.SetActive(true);
            jugador = other.GetComponent<ControladorCombate>();
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (yaUsoTotem) return;

        if (other.CompareTag("Player"))
        {
            canvasInteractuar.SetActive(false);
            jugadorCerca = false;
            jugador = null;
        }
    }

    public void AumentarVida()
    {
        jugador?.AumentarVidaTotem(100f);
        HUDJugador.SetActive(true);
        jugador.setAtacando(false);
        yaUsoTotem = true;
    }

    public void AumentarEstamina()
    {
        jugador?.AumentarEstaminaTotem(100f);
        HUDJugador.SetActive(true);
        jugador.setAtacando(false);
        yaUsoTotem = true;
    }
}
