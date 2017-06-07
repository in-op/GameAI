namespace GameAI
{
    public interface ICurrentPlayer
    {
        /// <summary>
        /// Gets the player whose turn it is in the current gamestate.
        /// </summary>
        int CurrentPlayer { get; }
    }
}