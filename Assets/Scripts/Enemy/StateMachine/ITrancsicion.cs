public interface ITrancsicion
{
    IEstado NuevoEstado { get; }
    IPredicate Condicion{ get; }
}