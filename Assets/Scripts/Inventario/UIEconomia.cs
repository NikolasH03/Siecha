using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIEconomia : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dineroPacoUI;
    [SerializeField] TextMeshProUGUI dineroTisqaUI;
    void Start()
    {
        dineroTisqaUI.text = InventarioEconomia.instance.getDinero().ToString();
        dineroPacoUI.text = InventarioEconomia.instance.getDinero().ToString();
    }

    public void RefrescarUI()
    {
        if (ControladorCambiarPersonaje.instance.getEsMuisca())
            dineroTisqaUI.text = InventarioEconomia.instance.getDinero().ToString();
        else
            dineroPacoUI.text = InventarioEconomia.instance.getDinero().ToString();
    }
}
