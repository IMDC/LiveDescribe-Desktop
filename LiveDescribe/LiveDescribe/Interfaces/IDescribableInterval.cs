namespace LiveDescribe.Interfaces
{
    /// <summary>
    /// Represents a period of time that can be described in some way
    /// </summary>
    public interface IDescribableInterval
    {
        string Text { set; get; }
    }
}
