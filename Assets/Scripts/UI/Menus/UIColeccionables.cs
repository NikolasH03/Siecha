using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UIColeccionables : MonoBehaviour
{
    [SerializeField] GameObject contenedor;
    [SerializeField] GameObject prefabItemUI;

    public event Action<List<Button>> OnColeccionablesInstanciados;
    public void RefrescarUI()
    {
        foreach (Transform hijo in contenedor.transform)
        {
            Destroy(hijo.gameObject);
        }

        List<Button> botones = new List<Button>();


        for (int i = 0; i < InventarioColeccionables.instance.TotalColeccionables(); i++)
        {
            GameObject item = Instantiate(prefabItemUI, contenedor.transform);
            ColeccionableData datos = InventarioColeccionables.instance.GetDatos(i);
            bool activo = InventarioColeccionables.instance.EstaDesbloqueado(datos.id);
            item.GetComponent<UIItemColeccionable>().Configurar(datos, activo);


            Button boton = item.GetComponent<Button>();
            if (boton != null)
                botones.Add(boton);
        }


        OnColeccionablesInstanciados?.Invoke(botones);
    }

    private void Start()
    {
        RefrescarUI(); 
    }
}
