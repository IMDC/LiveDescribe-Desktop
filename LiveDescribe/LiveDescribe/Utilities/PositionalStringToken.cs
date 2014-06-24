namespace LiveDescribe.Utilities
{
    /// <summary>
    /// Contains the positions and texts of a token from a larger string.
    /// </summary>
    public class PositionalStringToken
    {
        public PositionalStringToken(string token, int start, int end)
        {
            Text = token;
            StartIndex = start;
            EndIndex = end;
        }

        public string Text { private set; get; }
        public int StartIndex { private set; get; }
        public int EndIndex { private set; get; }

        public int Length
        {
            get { return Text.Length; }
        }
    }
}
