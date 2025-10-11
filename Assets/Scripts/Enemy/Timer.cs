using System;


public abstract class Timer
{
    protected float tiempoInicial;
    protected float Tiempo { get; set; }
    public bool EstaCorriendo { get; protected set; }

    public float Progreso => Tiempo / tiempoInicial;
    
    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer(float valorInicial)
    {
        tiempoInicial = valorInicial;
        EstaCorriendo = false;
    }

    public void Empezar()
    {
        Tiempo = tiempoInicial;
        if (!EstaCorriendo)
        {
            EstaCorriendo = true;
            OnTimerStart.Invoke();
        }
    }

    public void Continuar() => EstaCorriendo = true;
    public void Pausar() => EstaCorriendo = false;
    
    public abstract void Tick(float deltaTime);
}

public class Temporizador : Timer
{
    public Temporizador(float valorInicial) : base(valorInicial) {}

    public override void Tick(float deltaTime)
    {
        if (EstaCorriendo && Tiempo > 0)
        {
            Tiempo -= deltaTime;
        }

        if (EstaCorriendo && Tiempo <= 0)
        {
            Pausar();
        }
    }
    
    public bool HaFinalizado => Tiempo <= 0;
    
    public void Reiniciar() => Tiempo = tiempoInicial;

    public void Reiniciar(float nuevoValor)
    {
        tiempoInicial = nuevoValor;
        Reiniciar();
    }
}