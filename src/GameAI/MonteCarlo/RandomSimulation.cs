using System;
using System.Collections.Generic;
using SystemExtensions;
using SystemExtensions.Random;

namespace GameAI.MonteCarlo
{
    public static class RandomSimulation
    {
        /// <summary>
        /// Interface for games that wish
        /// to use the MonteCarlo AI.
        /// </summary>
        /// <typeparam name="TMove">The type of the moves in the IGame implementation.</typeparam>
        public interface IGame<TMove>
        {
            /// <summary>
            /// Returns a list of all legal moves
            /// possible from the current gamestate.
            /// </summary>
            List<TMove> GetLegalMoves();
            /// <summary>
            /// Execute the move and update the gamestate.
            /// </summary>
            /// <param name="move">The move to perform.</param>
            void DoMove(TMove move);
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
            IGame<TMove> DeepCopy();
            /// <summary>
            /// Returns the current player.
            /// </summary>
            int GetCurrentPlayer();
        }



        public static TMove Search<TMove>(IGame<TMove> game, int simulations, bool useParallel)
        {
            if (useParallel)
            {

            }
            else
            {
                return Search(game, simulations);
            }
        }


        private class LocalLoopVariables<TMove>
        {
            public int aiPlayer;
            public List<TMove> legalMoves;
            public int count;
            public MoveStats[] moveStats;
            public int moveIndex;
            public IGame<TMove> copy;
            public Random rng;

            public LocalLoopVariables(int aiPlayer, List<TMove> legalMoves, int count, MoveStats[] moveStats, int moveIndex, IGame<TMove> copy, Random rng)
            {
                this.aiPlayer = aiPlayer;
                this.legalMoves = legalMoves;
                this.count = count;
                this.moveStats = moveStats;
                this.moveIndex = moveIndex;
                this.copy = copy;
                this.rng = rng;
            }
        }

        public static TMove ParallelSearch<TMove>(IGame<TMove> game, int simulations)
        {
            List<TMove> legalMoves = game.GetLegalMoves();


        }



        /// <summary>
        /// Returns the move found to have highest win-rate
        /// after performing the specified number of
        /// Monte-Carlo simulations on the input game.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        /// <typeparam name="TMove">The type of the moves in the IGame implementation.</typeparam>
            public static TMove Search<TMove>(IGame<TMove> game, int simulations)
        {
            // hoist all declarations out of the main loop for performance
            int aiPlayer = game.GetCurrentPlayer();
            List<TMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = JaggedArray.Create(count, new MoveStats());
            int moveIndex;
            IGame<TMove> copy;
            Random rng = new Random();

            for (int i = 0; i < simulations; i++)
            {
                moveIndex = rng.Next(0, count);
                copy = game.DeepCopy();
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
