using System;
using System.Collections.Generic;
using UnityEngine;

public class MaquinaDeEstados
{
    NodoDeEstados current;
    private Dictionary<Type, NodoDeEstados> nodos = new();
    HashSet<ITrancsicion> transicionesGlobales = new();

    public void Update()
    {
        //Verificar que hay un estado actual
        if (current == null || current.Estado == null)
        {
            Debug.LogWarning("[MaquinaDeEstados] No hay estado actual establecido");
            return;
        }

        var transicion = GetTransicion();
        if (transicion != null)
            CambiarEstado(transicion.NuevoEstado);

        current.Estado?.Update();
    }

    //Ahora puede recibir estados que no están en el diccionario
    public void SetEstado(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando establecer un estado null");
            return;
        }

        // Si el estado no existe en nodos, agregarlo
        var nodo = ObtenerOAgregarNodo(estado);

        current = nodo;
        current.Estado?.OnEnter();
    }

    //Manejo seguro de estados no registrados
    public void CambiarEstado(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando cambiar a un estado null");
            return;
        }

        // Si es el mismo estado, no hacer nada
        if (current != null && estado == current.Estado) return;

        // Obtener o crear el nodo del nuevo estado
        var nuevoNodo = ObtenerOAgregarNodo(estado);

        // Salir del estado anterior si existe
        if (current != null && current.Estado != null)
        {
            current.Estado.OnExit();
        }

        // Entrar al nuevo estado
        current = nuevoNodo;
        current.Estado?.OnEnter();
    }

    ITrancsicion GetTransicion()
    {
        //Verificación de seguridad
        if (current == null) return null;

        // Primero verificar transiciones globales
        foreach (var transicion in transicionesGlobales)
            if (transicion.Condicion.Evaluate())
                return transicion;

        // Luego verificar transiciones locales del estado actual
        if (current.Transiciones != null)
        {
            foreach (var transicion in current.Transiciones)
                if (transicion.Condicion.Evaluate())
                    return transicion;
        }

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

    //Ahora retorna el nodo en lugar de solo agregarlo
    NodoDeEstados ObtenerOAgregarNodo(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando obtener/agregar un estado null");
            return null;
        }

        Type tipoEstado = estado.GetType();

        // Si el nodo ya existe, retornarlo
        if (nodos.TryGetValue(tipoEstado, out var nodoExistente))
        {
            return nodoExistente;
        }

        // Si no existe, crear uno nuevo
        var nuevoNodo = new NodoDeEstados(estado);
        nodos.Add(tipoEstado, nuevoNodo);

        return nuevoNodo;
    }

    public void FixedUpdate()
    {
        current?.Estado?.FixedUpdate();
    }

    //Método para verificar si hay un estado actual
    public bool TieneEstadoActual()
    {
        return current != null && current.Estado != null;
    }

    //Obtener el estado actual (útil para debug)
    public IEstado ObtenerEstadoActual()
    {
        return current?.Estado;
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