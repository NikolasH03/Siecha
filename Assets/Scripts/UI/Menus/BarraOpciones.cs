//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using TMPro;
//using DG.Tweening;

//public class BarraOpciones : MenuBaseConNavegacion
//{
//    [Header("Barra de Tabs Superior")]
//    [SerializeField] private TextMeshProUGUI textoVolumen;
//    [SerializeField] private TextMeshProUGUI textoControles;
//    [SerializeField] private TextMeshProUGUI textoGraficos;
//    [SerializeField] private Image selectorVisual;

//    private int opcionActual = 0;
//    private string[] nombresOpciones = { "Volumen", "Controles", "Graficos" };
//    private TextMeshProUGUI[] textosOpciones;

//    [Header("Botones de Acción")]
//    [SerializeField] private Button botonGuardar;
//    [SerializeField] private Button botonVolver;

//    private void Awake()
//    {
//        textosOpciones = new TextMeshProUGUI[] { textoVolumen, textoControles, textoGraficos };
//    }

//    protected override void OnMenuOpened()
//    {
//        opcionActual = 0;

//        ActualizarOpcion();
//    }
//    protected override void ConfigurarNavegacion()
//    {
//        if (botonGuardar && botonVolver)
//        {

//            ConfigurarNavegacionBoton(botonGuardar,
//                derecha: botonVolver, izquierda: botonVolver);

//            ConfigurarNavegacionBoton(botonVolver,
//                izquierda: botonGuardar, derecha: botonGuardar);

//            primerSeleccionable = botonGuardar;
//        }
//    }

//    private void Update()
//    {
//        if (!IsOpen) return;

//        // Cambiar entre opciones con RB/LB
//        if (InputJugador.instance?.CambiarTabDerecha == true)
//        {
//            CambiarTab(1);
//        }

//        if (InputJugador.instance?.CambiarTabIzquierda == true)
//        {
//            CambiarTab(-1);
//        }
//    }

//    private void CambiarTab(int direccion)
//    {
//        opcionActual += direccion;

//        if (opcionActual >= nombresOpciones.Length)
//            opcionActual = 0;
//        else if (opcionActual < 0)
//            opcionActual = nombresOpciones.Length - 1;

//        ActualizarOpcion();
//    }

//    private void ActualizarOpcion()
//    {

//        switch (opcionActual)
//        {
//            case 0: // Volumen
//                AbrirVolumen();       
//                break;

//            case 1: // Controles
//                AbrirControles();
//                break;

//            case 2: // Pantalla Completa
//                 AbrirGraficos();
//                break;
//        }

//        Debug.Log($"Tab actual: {nombresOpciones[opcionActual]}");
//    }


//}
