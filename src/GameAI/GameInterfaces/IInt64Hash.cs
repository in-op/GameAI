namespace GameAI.GameInterfaces
{
    public interface IInt64Hash
    {
        /// <summary>
        /// Returns a 64-bit hash value representing the current, distinct gamestate.
        /// </summary>
        long Hash { get; }
    }
}