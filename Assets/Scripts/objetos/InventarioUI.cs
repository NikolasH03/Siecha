using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventarioUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dineroUI;
    void Start()
    {
        dineroUI.text = Inventario.instance.getDinero().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        dineroUI.text = Inventario.instance.getDinero().ToString();
    }
}
