using System.Collections.Generic;

namespace GameAI
{
    /// <summary>
    /// A method class to select moves in games
    /// that are two-player, back-and-forth,
    /// deterministic, and zero-sum or zero-sum-tie.
    /// </summary>
    public static class MiniMax
    {
        /// <summary>
        /// An interface for Games that
        /// wish to use the MiniMax AI.
        /// </summary>
        public interface Game
        {
            /// <summary>
            /// Returns a list of legal moves
            /// for the current gamestate.
            /// </summary>
            List<Move> GetLegalMoves();
            /// <summary>
            /// Perform the input move on
            /// the game and return the updated
            /// game.
            /// </summary>
            /// <param name="move">The move to perform.</param>
            Game DoMove(Move move);
            /// <summary>
            /// Update the gamestate to
            /// completely undo the previous move.
            /// </summary>
            void UndoMove();
            /// <summary>
            /// Returns whether the game is over or not.
            /// </summary>
            bool IsGameOver();
            /// <summary>
            /// Return the score for the player whos
            /// turn it is in this current gamestate.
            /// </summary>
            int CurrentPlayersScore();
        }
        /// <summary>
        /// Blank interface for Move objects
        /// of the game.
        /// </summary>
        public interface Move { }

        /// <summary>
        /// Returns the best Move found by performing
        /// a full MiniMax gamestate search.
        /// </summary>
        /// <param name="game">The gamestate from which to begin the search.</param>
        public static Move Search(Game game)
        {
            int bestScore = int.MinValue;
            Move bestMove = null;
            int score;
            foreach (Move move in game.GetLegalMoves())
            {
                score = -NegaMax(game.DoMove(move));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                game.UndoMove();
            }
            return bestMove;
        }

        private static int NegaMax(Game game)
        {
            if (game.IsGameOver())
                return game.CurrentPlayersScore();

            int bestScore = int.MinValue;
            int score;
            foreach (Move move in game.GetLegalMoves())
            {
                score = -NegaMax(game.DoMove(move));
                if (score > bestScore) bestScore = score;
                game.UndoMove();
            }
            return bestScore;
        }
    }
}
