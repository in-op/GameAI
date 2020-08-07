namespace GameAI.GameInterfaces
{
    public interface ICurrentPlayer<TPlayer>
    {
        /// <summary>
        /// Gets the player whose turn it is in the current gamestate.
        /// </summary>
        TPlayer CurrentPlayer { get; }
    }
}
