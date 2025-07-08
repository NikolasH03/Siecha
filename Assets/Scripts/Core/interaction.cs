using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class interaction : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] GameObject conversation;
    [SerializeField] GameObject conversationCanvas;
    [SerializeField] GameObject buttons;
    [SerializeField] ControladorCombate player;
    private bool playerInRange;
    private int numConversation=0;
    

    void Start()
    {
        text.SetActive(false);
        conversationCanvas.SetActive(false);
        buttons.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        text.SetActive(true);
        playerInRange = true;
        
    }
    private void OnTriggerExit(Collider other)
    {
        text.SetActive(false);
        playerInRange = false;
        conversationCanvas.SetActive(false);
        buttons.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    void Update()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        player = playerObject.GetComponent<ControladorCombate>();

        if (playerInRange)
        {
            TextMeshProUGUI textComponent = text.GetComponent<TextMeshProUGUI>();
            textComponent.text = "'E' para hablar con el espiritu";
        }
        if(Input.GetKeyDown(KeyCode.E) && playerInRange) {
            showNextDialogue();
        }
        
    }
    public void showNextDialogue()
    {
        
        text.SetActive(false);
        TextMeshProUGUI textConversation = conversation.GetComponent<TextMeshProUGUI>();

        switch (numConversation)
        {

            case 0:
                numConversation++;
                conversationCanvas.SetActive(true);
                textConversation.text = "el camino a la guarida del Mohan es subiendo la colina, el Mohan hace parte de nuestro ecosistema";
                break;
            case 1:
                numConversation++;
                conversationCanvas.SetActive(true);
                textConversation.text = "¿planeas eliminar al Mohan?";
                break;
            case 2:

                buttons.SetActive(true);
                conversationCanvas.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                player.enabled = false;

                break;

            case 3:

                buttons.SetActive(false);
                conversationCanvas.SetActive(true);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                player.enabled = true;
                textConversation.text = "Que Chiminigagua se apiade de ti cuando se entere";
                break;

            case 4:

                buttons.SetActive(false);
                conversationCanvas.SetActive(true);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                player.enabled = true;
                textConversation.text = "Sabia desición, hay fuerzas con las que no debemos meternos";
                break;
        }
    }
    public void goodKarma()
    {
        numConversation=4;
        showNextDialogue();
    }
    public void badKarma() {  
        numConversation=3;
        showNextDialogue();
    }
}
