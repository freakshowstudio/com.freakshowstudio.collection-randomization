
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;


namespace FreakshowStudio.CollectionRandomization.Runtime
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Randomly shuffles the elements of the given enumerable sequence
        /// and returns a new sequence with the elements in shuffled order.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the enumerable.
        /// </typeparam>
        /// <param name="source">
        /// The enumerable sequence to shuffle.
        /// </param>
        /// <returns>
        /// A new enumerable sequence with the elements shuffled randomly.
        /// </returns>
        public static IEnumerable<T> Shuffle<T>(
            this IEnumerable<T> source)
        {
            var array = source.ToArray();
            array.Shuffle();

            foreach (var item in array)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Selects a random subset of a specified size from the given
        /// enumerable sequence using reservoir sampling (Algorithm L).
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the enumerable.
        /// </typeparam>
        /// <param name="source">
        /// The enumerable sequence from which the subset will be sampled.
        /// </param>
        /// <param name="k">
        /// The size of the subset to select.
        /// </param>
        /// <returns>
        /// A new enumerable sequence containing a randomly selected
        /// subset of the specified size.
        /// </returns>
        public static IEnumerable<T> ReservoirSample<T>(
            this IEnumerable<T> source,
            int k)
        {
            int ComputeLog(float w) =>
                FloorToInt(Log(Random.value) / Log(1 - w));

            var reservoir = new List<T>(k);
            using var enumerator = source.GetEnumerator();

            for (var i = 0; i < k; i++)
            {
                if (!enumerator.MoveNext()) return reservoir.Shuffle();
                reservoir.Add(enumerator.Current);
            }

            var w = Exp(Log(Random.value) / k);
            var next = k + ComputeLog(w);
            var index = k;

            while (enumerator.MoveNext())
            {
                if (index == next)
                {
                    reservoir[Random.Range(0, k)] = enumerator.Current;
                    w *= Exp(Log(Random.value) / k);
                    next += ComputeLog(w) + 1;
                }

                index++;
            }

            return reservoir.Shuffle();
        }

        /// <summary>
        /// Selects and returns a random element from the given enumerable
        /// sequence.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the enumerable.
        /// </typeparam>
        /// <param name="source">
        /// The enumerable sequence from which to select a random element.
        /// </param>
        /// <returns>
        /// A randomly selected element from the source sequence.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the source sequence contains no elements.
        /// </exception>
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            if (source is T[] array)
            {
                if (array.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Sequence contains no elements");
                }

                return array[Random.Range(0, array.Length)];
            }

            if (source is IList<T> list)
            {
                if (list.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Sequence contains no elements");
                }

                return list[Random.Range(0, list.Count)];
            }

            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException(
                    "Sequence contains no elements");
            }

            var result = enumerator.Current;
            var count = 1;

            while (enumerator.MoveNext())
            {
                count++;
                if (Random.Range(0, count) == 0)
                {
                    result = enumerator.Current;
                }
            }

            return result;
        }

        /// <summary>
        /// Selects a random element from the given enumerable sequence
        /// based on the weights provided by the weight selector function.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the enumerable.
        /// </typeparam>
        /// <param name="source">
        /// The enumerable sequence from which to select an element.
        /// </param>
        /// <param name="weightSelector">
        /// A function that takes an element of the sequence and
        /// returns its weight.
        /// </param>
        /// <returns>
        /// A randomly selected element, weighted by the values
        /// provided by the weight selector function.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the sequence is empty or if the total weight of
        /// the elements is not positive.
        /// </exception>
        public static T WeightedRandomElement<T>(
            this IEnumerable<T> source,
            Func<T, float> weightSelector)
        {
            var items = source as T[] ?? source.ToArray();

            if (items.Length == 0)
            {
                throw new InvalidOperationException(
                    "Sequence contains no elements");
            }

            var totalWeight = 0f;
            var weights = new float[items.Length];

            for (var i = 0; i < items.Length; i++)
            {
                var weight = weightSelector(items[i]);
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0)
            {
                throw new InvalidOperationException(
                    "Total weight must be positive");
            }

            var randomValue = Random.value * totalWeight;
            var currentWeight = 0f;

            for (var i = 0; i < items.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    return items[i];
                }
            }

            return items[^1];
        }

        /// <summary>
        /// Selects a specified number of elements from the given enumerable
        /// sequence based on weighted probabilities.
        ///
        /// The selection can optionally be performed with or
        /// without replacement.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the enumerable.
        /// </typeparam>
        /// <param name="source">
        /// The enumerable sequence to select elements from.
        /// </param>
        /// <param name="weightSelector">
        /// A function that determines the weight of each element.
        /// </param>
        /// <param name="count">
        /// The number of elements to select.
        /// </param>
        /// <param name="replacement">
        /// A boolean indicating whether selection is performed with
        /// replacement.
        ///
        /// If true, an element may be selected more than once;
        /// otherwise, it cannot.
        /// </param>
        /// <returns>
        /// A new enumerable sequence containing the selected elements
        /// based on their weighted probabilities.
        /// </returns>
        public static IEnumerable<T> WeightedRandomElements<T>(
            this IEnumerable<T> source,
            Func<T, float> weightSelector,
            int count,
            bool replacement = true)
        {
            var items = source as T[] ?? source.ToArray();

            if (replacement)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return items.WeightedRandomElement(weightSelector);
                }
            }
            else
            {
                var weights = items.Select(weightSelector).ToArray();
                var totalWeight = weights.Sum();
                var min = Min(count, items.Length);

                for (var i = 0; i < min; i++)
                {
                    if (totalWeight <= 0) yield break;

                    var randomValue = Random.value * totalWeight;
                    var currentWeight = 0f;

                    for (var j = 0; j < items.Length; j++)
                    {
                        if (weights[j] <= 0) continue;

                        currentWeight += weights[j];

                        if (randomValue <= currentWeight)
                        {
                            yield return items[j];
                            totalWeight -= weights[j];
                            weights[j] = 0;
                            break;
                        }
                    }
                }
            }
        }
    }
}
