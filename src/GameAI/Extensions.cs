using System;
using System.Collections.Generic;

namespace GameAI
{
    internal static class Extensions
    {
        /// <summary>
        /// Return a random element from the List.
        /// </summary>
        /// <typeparam name="T">The type of elements in the List.</typeparam>
        /// <param name="list">The calling List.</param>
        /// <param name="random">An instance of the Random class.</param>
        internal static T RandomItem<T>(this List<T> list, Random random)
            => list[random.Next(0, list.Count)];
    }
}
