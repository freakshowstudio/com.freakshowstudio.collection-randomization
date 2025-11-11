
using System.Collections.Generic;
using Random = UnityEngine.Random;


namespace FreakshowStudio.CollectionRandomization.Runtime
{
    public static class ListExtensions
    {
        /// <summary>
        /// Randomly shuffles the elements of the list in place.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the list.
        /// </typeparam>
        /// <param name="list">
        /// The list to be shuffled.
        /// </param>
        public static void ShuffleInPlace<T>(this IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
