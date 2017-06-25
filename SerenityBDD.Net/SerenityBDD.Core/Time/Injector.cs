namespace SerenityBDD.Core.Time
{
    /// <summary>
    /// this should be some container configuration like autofac or similar
    /// </summary>
    public interface Injector
    {
        T getInstance<T>();
    }
}