using System;
using System.Collections.Generic;

public class Combo
{
    public List<TipoInputCombate> secuencia;
    public Func<CombatStateMachine, ControladorCombate, CombatState> crearEstado;

    public Combo(List<TipoInputCombate> secuencia, Func<CombatStateMachine, ControladorCombate, CombatState> crearEstado)
    {
        this.secuencia = secuencia;
        this.crearEstado = crearEstado;
    }
}

