using System.Collections.Generic;
using System.Threading.Tasks;
using GameAI.GameInterfaces;

namespace GameAI.Algorithms.MiniMax
{
    /// <summary>
    /// A static class to run a parallel MiniMax algorithm on a game.
    /// </summary>
    public static class ParallelTreeSearch<TGame, TMove, TPlayer>
        where TGame : ParallelTreeSearch<TGame, TMove, TPlayer>.IGame
    {
        /// <summary>
        /// Interface implemented by games to run the MiniMax <c cref="Search">Search</c> algorithm.
        /// </summary>
        public interface IGame :
            ICopyable<TGame>,
            IDoMove<TMove>,
            IGameOver,
            ILegalMoves<TMove>,
            IUndoMove,
            ICurrentPlayer<TPlayer>,
            IScore<TPlayer>
        { }

        /// <summary>
        /// Return the best <c cref="TMove">TMove</c> found by performing
        /// a full MiniMax gamestate search in parallel.
        /// </summary>
        /// <param name="game">The gamestate from which to begin the search.</param>
        public static TMove Search(TGame game)
        {
            object locker = new object();
            int bestScore = int.MinValue;
            TMove bestMove = default;
            List<TMove> moves = game.GetLegalMoves();

            Parallel.For(0, moves.Count,

                () => game.DeepCopy(),

                (i, state, copy) =>
                {
                    copy.DoMove(moves[i]);
                    int score = -NegaMax(copy);
                    lock (locker)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = moves[i];
                        }
                    }
                    copy.UndoMove();
                    return copy;
                },

                copy => { }
            );

            return bestMove;
        }

        private static int NegaMax(TGame game)
        {
            if (game.IsGameOver())
                return game.Score(game.CurrentPlayer);

            int bestScore = int.MinValue;
            int score;
            foreach (TMove move in game.GetLegalMoves())
            {
                game.DoMove(move);
                score = -NegaMax(game);
                if (score > bestScore)
                    bestScore = score;
                game.UndoMove();
            }
            return bestScore;
        }
    }
}
