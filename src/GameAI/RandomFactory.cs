using System;

namespace GameAI
{
    internal static class RandomFactory
    {
        private static readonly Random GlobalRandom = new Random();
        private static readonly object Lock = new object();

        internal static Random Create()
        {
            lock (Lock)
            {
                return new Random(GlobalRandom.Next());
            }
        }
    }
}
