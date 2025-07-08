using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PausaUI : MonoBehaviour
{
    public static PausaUI instance;
    [SerializeField] GameObject menuPausa;
    [SerializeField] GameObject Coleccionables;
    [SerializeField] bool pausado = false;


    public void Start()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (!pausado)
            {
                Pausar();
            }
            else {
                resumir();
            }

        }
    }
    public void Pausar()
    {
        menuPausa.SetActive(true);
        pausado = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void resumir()
    {
        menuPausa.SetActive(false);
        pausado = false;

        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void MostrarColeccionables()
    {
        Coleccionables.SetActive(true);
        menuPausa.SetActive(false);
    }
    public void EsconderColeccionables()
    {     
        menuPausa.SetActive(true);
        Coleccionables.SetActive(false);
    }
}
