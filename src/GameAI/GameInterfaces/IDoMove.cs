namespace GameAI.GameInterfaces
{
    public interface IDoMove<TMove>
    {
        /// <summary>
        /// Update the gamestate to reflect the execution of the move.
        /// </summary>
        /// <param name="move">The move to execute.</param>
        void DoMove(TMove move);
    }
}
