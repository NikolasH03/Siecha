using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FSM con nodos indexados por TIPO de estado (estado.GetType()): por eso los estados se deben
/// registrar siempre con su tipo concreto, y una instancia nueva del mismo tipo reutiliza el nodo existente.
/// </summary>
public class MaquinaDeEstados
{
    private NodoDeEstados current;
    private readonly Dictionary<Type, NodoDeEstados> nodos = new();
    private readonly HashSet<ITrancsicion> transicionesGlobales = new();

    // ─── Ciclo de vida ────────────────────────────────────────────────────────

    public void Update()
    {
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

    public void FixedUpdate() => current?.Estado?.FixedUpdate();

    // ─── Cambio de estado ─────────────────────────────────────────────────────

    /// <summary>Establece el estado inicial (acepta estados aún no registrados en la FSM).</summary>
    public void SetEstado(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando establecer un estado null");
            return;
        }

        current = ObtenerOAgregarNodo(estado);
        current.Estado?.OnEnter();
    }

    public void CambiarEstado(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando cambiar a un estado null");
            return;
        }

        if (current != null && estado == current.Estado) return;

        var nuevoNodo = ObtenerOAgregarNodo(estado);

        if (current != null && current.Estado != null)
            current.Estado.OnExit();

        current = nuevoNodo;
        current.Estado?.OnEnter();
    }

    // ─── Transiciones ─────────────────────────────────────────────────────────

    public void AgregarTransicionGlobal(IEstado nuevoEstado, IPredicate condicion) =>
        transicionesGlobales.Add(new Trancsicion(ObtenerOAgregarNodo(nuevoEstado).Estado, condicion));

    public void AgregarTransicion(IEstado estadoActual, IEstado estadoSiguiente, IPredicate condicion) =>
        ObtenerOAgregarNodo(estadoActual).AgregarTransicion(ObtenerOAgregarNodo(estadoSiguiente).Estado, condicion);

    /// <summary>Las globales tienen prioridad sobre las del estado actual.</summary>
    private ITrancsicion GetTransicion()
    {
        if (current == null) return null;

        foreach (var transicion in transicionesGlobales)
            if (transicion.Condicion.Evaluate())
                return transicion;

        if (current.Transiciones != null)
            foreach (var transicion in current.Transiciones)
                if (transicion.Condicion.Evaluate())
                    return transicion;

        return null;
    }

    private NodoDeEstados ObtenerOAgregarNodo(IEstado estado)
    {
        if (estado == null)
        {
            Debug.LogError("[MaquinaDeEstados] Intentando obtener/agregar un estado null");
            return null;
        }

        Type tipoEstado = estado.GetType();
        if (nodos.TryGetValue(tipoEstado, out var nodoExistente))
            return nodoExistente;

        var nuevoNodo = new NodoDeEstados(estado);
        nodos.Add(tipoEstado, nuevoNodo);
        return nuevoNodo;
    }

    // ─── Consultas ────────────────────────────────────────────────────────────

    public bool TieneEstadoActual() => current != null && current.Estado != null;

    public IEstado ObtenerEstadoActual() => current?.Estado;

    // ─── Nodo ─────────────────────────────────────────────────────────────────

    private class NodoDeEstados
    {
        public IEstado Estado { get; }
        public HashSet<ITrancsicion> Transiciones { get; } = new HashSet<ITrancsicion>();

        public NodoDeEstados(IEstado estado) => Estado = estado;

        public void AgregarTransicion(IEstado nuevoEstado, IPredicate condicion) =>
            Transiciones.Add(new Trancsicion(nuevoEstado, condicion));
    }
}