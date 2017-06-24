namespace SerenityBDD.Core.Steps
{
    public interface IStepFactory
    {
        object GetUniqueStepLibraryFor(object clazz, object[] constructorParameters);
    }
}
