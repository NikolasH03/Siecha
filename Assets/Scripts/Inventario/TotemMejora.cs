using System.Collections.Generic;
using UnityEngine;

public class TotemMejora : MonoBehaviour
{
    [SerializeField] private GameObject canvasUI;
    [SerializeField] private GameObject canvasInteractuar;
    [SerializeField] private GameObject HUDJugador;
    private bool jugadorCerca = false;
    private bool yaUsoTotem = false;
    private ControladorCombate jugador;

    private void Awake()
    {
        canvasInteractuar.SetActive(false);
        canvasUI.SetActive(false);
    }
    private void Update()
    {
        if (yaUsoTotem) return;

        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            canvasUI.SetActive(true);
            HUDJugador.SetActive(false);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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
            canvasUI.SetActive(false);
            jugadorCerca = false;
            jugador = null;
        }
    }

    public void AumentarVida()
    {
        jugador?.AumentarVidaTotem(100f);
        canvasUI.SetActive(false);
        HUDJugador.SetActive(true);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        jugador.setAtacando(false);
        yaUsoTotem = true;
    }

    public void AumentarEstamina()
    {
        jugador?.AumentarEstaminaTotem(100f);
        canvasUI.SetActive(false);
        HUDJugador.SetActive(true);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        jugador.setAtacando(false);
        yaUsoTotem = true;
    }
}
