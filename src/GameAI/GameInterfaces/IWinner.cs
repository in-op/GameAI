namespace GameAI.GameInterfaces
{
    public interface IWinner<TPlayer>
    {
        /// <summary>
        /// Return whether the player has achieved the win conditions in the current gamestate.
        /// </summary>
        /// <param name="player">The player for whom to check victory.</param>
        /// <returns>True if the player has achieved the win conditions in the current gamestate, false otherwise.</returns>
        bool IsWinner(TPlayer player);
    }
}
