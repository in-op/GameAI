using System;
using System.Threading;

namespace GameAI
{
    internal static class RandomFactory
    {
        private static readonly Random GlobalRandom = new Random();

        internal static ThreadLocal<Random> Create()
            => new ThreadLocal<Random>(
                () => new Random(GlobalRandom.Next()));
    }
}
