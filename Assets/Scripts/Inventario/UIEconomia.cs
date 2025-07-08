using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIEconomia : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dineroUI;
    void Start()
    {
        dineroUI.text = InventarioEconomia.instance.getDinero().ToString();
    }

    void Update()
    {
        dineroUI.text = InventarioEconomia.instance.getDinero().ToString();
    }
}
