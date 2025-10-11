using System;
using System.Collections.Generic;
using UnityEngine;

public class MaquinaDeEstados
{
    NodoDeEstados current;
    private Dictionary<Type, NodoDeEstados> nodos = new();
    HashSet<ITrancsicion> transicionesGlobales = new(); // Cambiado a nombre más claro

    public void Update()
    {
        var transicion = GetTransicion();
        if (transicion != null)
            CambiarEstado(transicion.NuevoEstado);
        
        current.Estado?.Update();
    }

    public void SetEstado(IEstado estado)
    {
        current = nodos[estado.GetType()];
        current.Estado?.OnEnter();
    }

    public void CambiarEstado(IEstado estado)
    {
        if (estado == current.Estado) return;
        
        var estadoAnterior = current.Estado;
        var estadoSiguiente = nodos[estado.GetType()].Estado;
        
        estadoAnterior?.OnExit();
        estadoSiguiente?.OnEnter();
        current = nodos[estado.GetType()];
    }

    ITrancsicion GetTransicion()
    {
        foreach (var transicion in transicionesGlobales)
            if (transicion.Condicion.Evaluate())
                return transicion;

        foreach (var transicion in current.Transiciones)
            if(transicion.Condicion.Evaluate())
                return transicion;
        
        return null;
    }

    public void AgregarTransicionGlobal(IEstado nuevoEstado, IPredicate condicion)
    {
        transicionesGlobales.Add(new Trancsicion(ObtenerOAgregarNodo(nuevoEstado).Estado, condicion));
    }

    public void AgregarTransicion(IEstado estadoActual, IEstado estadoSiguiente, IPredicate condicion)
    {
        ObtenerOAgregarNodo(estadoActual).AgregarTransicion(ObtenerOAgregarNodo(estadoSiguiente).Estado, condicion);
    }

    NodoDeEstados ObtenerOAgregarNodo(IEstado estado)
    {
        var nodo = nodos.GetValueOrDefault(estado.GetType());

        if (nodo == null) // Corregido el condicional
        {
            nodo = new NodoDeEstados(estado);
            nodos.Add(estado.GetType(), nodo);
        }
        
        return nodo;
    }

    public void FixedUpdate()
    {
        current.Estado?.FixedUpdate();
    }

    class NodoDeEstados
    {
        public IEstado Estado { get; }
        public HashSet<ITrancsicion> Transiciones { get; }

        public NodoDeEstados(IEstado estado)
        {
            Estado = estado;
            Transiciones = new HashSet<ITrancsicion>();
        }

        public void AgregarTransicion(IEstado nuevoEstado, IPredicate condicion)
        {
            Transiciones.Add(new Trancsicion(nuevoEstado, condicion));
        }
    }
}