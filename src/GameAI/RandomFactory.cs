using System;

namespace GameAI
{
    internal static class RandomFactory
    {
        private static readonly Random GlobalRandom = new Random();
        private static readonly object Lock = new object();

        /// <summary>
        /// Create a new <c cref="Random">Random</c> instance with a random seed that is safe to
        /// use alongside others created with this same method as thread-local instances. The
        /// naive process of generating <c cref="Random">Random</c> instances back-to-back (often
        /// in a loop) without this safety can generate instances with a sufficiently similar or
        /// even the equal seeds such that querying them in parallel produces the same values.
        /// </summary>
        internal static Random Create()
        {
            lock (Lock)
            {
                return new Random(GlobalRandom.Next());
            }
        }
    }
}
