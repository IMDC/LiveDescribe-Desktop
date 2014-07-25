namespace LiveDescribe.Interfaces
{
    public interface ICopy<out T>
    {
        /// <summary>
        /// Makes a shallow copy of this instance.
        /// </summary>
        /// <returns>A shallow copy this instances type.</returns>
        T ShallowCopy();

        /// <summary>
        /// Makes a deep copy of this instance.
        /// </summary>
        /// <returns>A deep copy of this type</returns>
        T DeepCopy();
    }
}
