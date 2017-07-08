namespace SerenityBDD.Core.External
{
    public interface Wait<T> : Wait { }

    public interface Wait {
        void until(ExpectedCondition<bool> condition);
    }
}