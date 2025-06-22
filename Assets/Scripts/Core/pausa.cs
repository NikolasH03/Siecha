using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class pausa : MonoBehaviour
{

    [SerializeField] GameObject menuPausa;
    [SerializeField] bool pausado = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
            if(!pausado)
            {
                menuPausa.SetActive(true);
                pausado = true;
                Time.timeScale = 0;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else {
                resumir();
            }

        }
    }

    public void resumir()
    {
        menuPausa.SetActive(false);
        pausado=false;

        Time.timeScale = 1;
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
    }
}
