namespace SerenityBDD.Core.Steps
{
    public interface Wait {
        void until(ExpectedCondition<bool> condition);
    }
}