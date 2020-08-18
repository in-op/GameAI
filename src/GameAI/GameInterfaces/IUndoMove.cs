namespace GameAI.GameInterfaces
{
    public interface IUndoMove
    {
        /// <summary>
        /// Transform the game back to its previous state before the most recent move was executed.
        /// </summary>
        void UndoMove();
    }
}
