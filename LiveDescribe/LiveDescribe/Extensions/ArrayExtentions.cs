namespace LiveDescribe.Extensions
{
    public static class ArrayExtentions
    {
        /// <summary>
        /// Checks to see if the elements of each array are equal to each other.
        /// </summary>
        /// <typeparam name="T">The type of the arrays</typeparam>
        /// <param name="array">First array to compare.</param>
        /// <param name="other">Second array to compare.</param>
        /// <returns>True iff both arrays are of equal length and have the same elements in the
        /// same order.</returns>
        public static bool ElementsEquals<T>(this T[] array, T[] other)
        {
            if (array.Length != other.Length)
                return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].Equals(other[i]))
                    return false;
            }
            return true;
        }
    }
}
