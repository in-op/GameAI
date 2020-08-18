namespace GameAI.GameInterfaces
{
    public interface IRollout
    {
        /// <summary>
        /// Play the game until it ends. For performance, implementers may omit updating an
        /// internal hash value of the changing gamestate while this runs for performance.
        /// </summary>
        void Rollout();
    }
}
