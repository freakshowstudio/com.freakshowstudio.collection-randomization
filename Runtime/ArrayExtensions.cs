
using Random = UnityEngine.Random;


namespace FreakshowStudio.CollectionRandomization.Runtime
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Randomly shuffles the elements of the array in place.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the array.
        /// </typeparam>
        /// <param name="array">
        /// The array to shuffle.
        /// </param>
        public static void Shuffle<T>(this T[] array)
        {
            for (var i = array.Length - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}
