namespace GameAI.GameInterfaces
{
    public interface IDoMove<TMove>
    {
        /// <summary>
        /// Updates the game's internal representation to reflect the execution of the input move.
        /// </summary>
        /// <param name="move">The move to perform.</param>
        void DoMove(TMove move);
    }
}
