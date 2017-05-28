using System;
using System.Collections.Generic;
using SystemExtensions.Random;

namespace GameAI.MonteCarlo
{
    public static class RandomSimulationSearch
    {
        /// <summary>
        /// Blank interface for Moves of Games.
        /// </summary>
        public interface IMove { }

        /// <summary>
        /// Interface for games that wish
        /// to use the MonteCarlo AI.
        /// </summary>
        public interface IGame
        {
            /// <summary>
            /// Returns a list of all legal moves
            /// possible from the current gamestate.
            /// </summary>
            List<IMove> GetLegalMoves();
            /// <summary>
            /// Execute the move and update the gamestate.
            /// </summary>
            /// <param name="move">The move to perform.</param>
            void DoMove(IMove move);
            /// <summary>
            /// Returns whether the game is over or not.
            /// </summary>
            bool IsGameOver();
            /// <summary>
            /// Returns whether the input player
            /// is a winner in the current gamestate.
            /// </summary>
            /// <param name="player">The player.</param>
            bool IsWinner(int player);
            /// <summary>
            /// Returns a deep copy of the game.
            /// </summary>
            IGame Copy();
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
        public static IMove CalculateBestMove(IGame game, int simulations)
        {
            // hoist all declarations out of the main loop for performance
            int aiPlayer = game.GetCurrentPlayer();
            List<IMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = new MoveStats[count];
            for (int i = 0; i < count; i++) moveStats[i] = new MoveStats();
            int moveIndex;
            IGame copy;
            Random rng = new Random();

            for (int i = 0; i < simulations; i++)
            {
                moveIndex = rng.Next(0, count);
                copy = game.Copy();
                copy.DoMove(legalMoves[moveIndex]);

                while (!copy.IsGameOver())
                    copy.DoMove(
                        copy.GetLegalMoves().RandomItem(rng));

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
        }
    }
}
