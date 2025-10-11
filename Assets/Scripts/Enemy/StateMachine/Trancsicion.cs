public class Trancsicion : ITrancsicion
{
    public IEstado NuevoEstado { get; }
    public IPredicate Condicion{ get; }

    public Trancsicion(IEstado e, IPredicate p)
    {
        NuevoEstado = e;
        Condicion = p;
    }
}