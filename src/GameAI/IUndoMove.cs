namespace GameAI
{
    public interface IUndoMove
    {
        /// <summary>
        /// Transforms the game back to its previous state before the last move was executed.
        /// </summary>
        void UndoMove();
    }
}
