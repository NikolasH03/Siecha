using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



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
    public void IrArbolHabilidades(string Nivel)
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.mus_arbol);
        SceneManager.LoadScene(Nivel);
    }
}
