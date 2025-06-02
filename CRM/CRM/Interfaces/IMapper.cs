namespace Interfaces;

public interface IDataMapper<TSource, TDestination> 
{
    TDestination Map(TSource source);
}