using System;
using System.Collections.Generic;

namespace LiveDescribe.Utilities
{
    public class PositionalStringTokenizer
    {
        private string SourceString { set; get; }

        public List<PositionalStringToken> Tokens { private set; get; }

        public PositionalStringTokenizer(string stringToTokenize)
        {
            SourceString = stringToTokenize;
            Tokens = new List<PositionalStringToken>();
        }

        /// <summary>
        /// Create a list of Positional Tokens
        /// </summary>
        public void Tokenize()
        {
            if (string.IsNullOrWhiteSpace(SourceString))
                return;

            var tokens = SourceString.Split();

            int currentIndex = 0;
            foreach (string token in tokens)
            {
                //Skip empty tokens
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                int begin = SourceString.IndexOf(token, currentIndex, StringComparison.Ordinal);
                int end = begin + token.Length;
                Tokens.Add(new PositionalStringToken(token, begin, end));

                currentIndex = end;
            }
        }
    }
}
