using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEstado
{
    void OnEnter();
    void Update();
    void FixedUpdate();
    void OnExit();
}