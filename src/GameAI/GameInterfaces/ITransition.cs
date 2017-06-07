namespace GameAI.GameInterfaces
{
    public interface ITransition<TTransition>
    {
        /// <summary>
        /// Execute the move and update the hash value.
        /// </summary>
        /// <param name="t">The transition to perform.</param>
        void Transition(TTransition t);
    }
}
