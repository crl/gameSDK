namespace gameSDK
{
    /// <summary>
    /// 目标;
    /// </summary>
    public interface IGoal
    {
        int getPriority();
        float execute();
    }
}