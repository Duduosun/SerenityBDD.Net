namespace SerenityBDD.Core.steps
{
    public interface IStepFactory
    {
        object GetUniqueStepLibraryFor(object clazz, object[] constructorParameters);
    }
}
