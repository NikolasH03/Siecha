using System;
using System.Collections.Generic;
using UnityEngine;

public static class ComboDatabase
{
    public static Dictionary<string, Combo> Combos = new Dictionary<string, Combo>();

    static ComboDatabase()
    {

        Combos.Add("Combo1", new Combo(
            new List<TipoInputCombate> {
                TipoInputCombate.Ligero,
                TipoInputCombate.Ligero,
                TipoInputCombate.Fuerte
            },
            (fsm, cc) => new Combo1(fsm, cc)
        ));

        Combos.Add("Combo2", new Combo(
            new List<TipoInputCombate> {
                TipoInputCombate.Fuerte,
                TipoInputCombate.Fuerte,
                TipoInputCombate.Ligero
            },
            (fsm, cc) => new Combo2(fsm, cc)
        ));

        
    }
}

