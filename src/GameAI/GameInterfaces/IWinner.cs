namespace GameAI.GameInterfaces
{
    public interface IWinner<TPlayer>
    {
        /// <summary>
        /// Returns whether the given player is victories in the current gamestate.
        /// </summary>
        /// <param name="player">The player for whom to check victory.</param>
        /// <returns>True if the player is victorious in the current gamestate, false otherwise.</returns>
        bool IsWinner(TPlayer player);
    }
}
