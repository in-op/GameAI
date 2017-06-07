using System.Collections.Generic;
using SystemExtensions.Copying;

namespace GameAI.MiniMax
{
    /// <summary>
    /// A method class to select moves in games
    /// that are two-player, back-and-forth,
    /// deterministic, and zero-sum or zero-sum-tie.
    /// </summary>
    public static class ParallelTreeSearch<TGame, TMove>
        where TGame : ParallelTreeSearch<TGame, TMove>.IGame
    {
        /// <summary>
        /// An interface for Games that wish to use the MiniMax AI.
        /// </summary>
        public interface IGame :
            ICopyable<TGame>,
            IDoMove<TMove>,
            IGameOver,
            ILegalMoves<TMove>,
            IUndoMove
        {
            /// <summary>
            /// Return the score for the player whos turn it is in this current gamestate.
            /// </summary>
            int CurrentPlayersScore();
        }

        /// <summary>
        /// Returns the best Move found by performing
        /// a full MiniMax gamestate search in parallel.
        /// </summary>
        /// <param name="game">The gamestate from which to begin the search.</param>
        public static TMove Search(TGame game)
        {
            object locker = new object();
            int bestScore = int.MinValue;
            TMove bestMove = default(TMove);
            List<TMove> moves = game.GetLegalMoves();

            ParallelNET35.Parallel.For(0, moves.Count,

                () => { return game.DeepCopy(); },

                delegate(int i, ParallelNET35.Parallel.ParallelLoopState state, TGame copy)
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

                (copy) => { });

            return bestMove;
        }

        private static int NegaMax(TGame game)
        {
            if (game.IsGameOver())
                return game.CurrentPlayersScore();

            int bestScore = int.MinValue;
            int score;
            foreach (TMove move in game.GetLegalMoves())
            {
                game.DoMove(move);
                score = -NegaMax(game);
                if (score > bestScore) bestScore = score;
                game.UndoMove();
            }
            return bestScore;
        }
    }
}
