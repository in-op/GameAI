using CSharpExtras;
using System;
using System.Collections.Generic;

namespace GameAI
{
    /// <summary>
    /// A method class for obtaining the
    /// best Move from any Game by performing
    /// Monte Carlo simulations.
    /// </summary>
    public static class MonteCarlo
    {
        /// <summary>
        /// Blank interface for Moves of Games
        /// </summary>
        public interface Move { }

        /// <summary>
        /// Interface for games that wish
        /// to use the MonteCarlo AI.
        /// </summary>
        public interface Game
        {
            /// <summary>
            /// Returns a list of all legal moves
            /// possible from the current gamestate.
            /// </summary>
            /// <returns></returns>
            List<Move> GetLegalMoves();
            /// <summary>
            /// Execute the move and update the gamestate.
            /// </summary>
            /// <param name="move">The move to perform.</param>
            void DoMove(Move move);
            /// <summary>
            /// Returns whether the game is over or not.
            /// </summary>
            bool IsGameOver();
            /// <summary>
            /// Returns whether the input player
            /// is a winner in the current gamestate.
            /// </summary>
            /// <param name="player">The player</param>
            /// <returns></returns>
            bool IsWinner(int player);
            /// <summary>
            /// Returns a deep copy of the game.
            /// </summary>
            Game Copy();
            /// <summary>
            /// Returns the current player.
            /// </summary>
            int GetCurrentPlayer();
        }



        /// <summary>
        /// Returns the move found to have highest win-rate
        /// after performing the specified number of
        /// Monte-Carlo simulations on the input game.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        public static Move Search(Game game, int simulations)
        {
            // hoist all declarations out of the main loop for performance
            int aiPlayer = game.GetCurrentPlayer();
            List<Move> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = new MoveStats[count];
            for (int i = 0; i < count; i++) moveStats[i] = new MoveStats();
            int moveIndex;
            Game copy;
            List<Move> copysLegalMoves;
            Random rng = new Random();

            for (int i = 0; i < simulations; i++)
            {
                moveIndex = rng.Next(0, count);
                copy = game.Copy();
                copy.DoMove(legalMoves[moveIndex]);

                while (!copy.IsGameOver())
                {
                    copysLegalMoves = copy.GetLegalMoves();
                    copy.DoMove(copysLegalMoves.RandomItem(rng));
                }

                moveStats[moveIndex].executions++;
                if (copy.IsWinner(aiPlayer)) moveStats[moveIndex].victories++;
            }

            int bestMoveFound = 0;
            float bestScoreFound = 0f;
            for (int i = 0; i < count; i++)
            {
                float score = moveStats[i].Score();
                if (score > bestScoreFound)
                {
                    bestScoreFound = score;
                    bestMoveFound = i;
                }
            }

            return legalMoves[bestMoveFound];
        }

        



        private class MoveStats
        {
            public float executions;
            public float victories;

            public float Score()
            {
                return victories / executions;
            }
            public MoveStats()
            {
                executions = 0f;
                victories = 0f;
            }
        }
    }
}
