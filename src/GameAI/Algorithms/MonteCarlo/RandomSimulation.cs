using System;
using System.Collections.Generic;
using SystemExtensions;
using SystemExtensions.Random;
using SystemExtensions.Copying;
using System.Threading;
using GameAI.GameInterfaces;

namespace GameAI.Algorithms.MonteCarlo
{
    public static class RandomSimulation<TGame, TMove, TPlayer>
        where TGame : RandomSimulation<TGame, TMove, TPlayer>.IGame
    {
        /// <summary>
        /// Interface for games that wish to use the MonteCarlo AI.
        /// </summary>
        public interface IGame :
            ICopyable<TGame>,
            IDoMove<TMove>,
            IGameOver,
            ICurrentPlayer<TPlayer>,
            ILegalMoves<TMove>,
            IWinner<TPlayer>
        { }
        


        /// <summary>
        /// Returns the move found to have highest win-rate after performing, in parallel, the specified number of Monte-Carlo simulations on the input game.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        public static TMove ParallelSearch(TGame game, int simulations)
        {
            TPlayer aiPlayer = game.CurrentPlayer;
            List<TMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = JaggedArray.Create(count, new MoveStats());

            ParallelNET35.Parallel.For(0, simulations,

                () => { return ThreadLocalRandom.NewRandom(); },

                (i, loop, localRandom) =>
            {
                int moveIndex = localRandom.Next(0, count);
                TGame copy = game.DeepCopy();
                copy.DoMove(legalMoves[moveIndex]);

                while (!copy.IsGameOver())
                    copy.DoMove(
                        copy.GetLegalMoves().RandomItem(localRandom));

                Interlocked.Add(ref moveStats[moveIndex].executions, 1);
                if (copy.IsWinner(aiPlayer))
                    Interlocked.Add(ref moveStats[moveIndex].victories, 1);

                return localRandom;
            },

                (x) => { }

            );

            
            int bestMoveFound = 0;
            double bestScoreFound = 0f;
            for (int i = 0; i < count; i++)
            {
                double score = moveStats[i].Score();
                if (score > bestScoreFound)
                {
                    bestScoreFound = score;
                    bestMoveFound = i;
                }
            }

            //for (int i = 0; i < legalMoves.Count; i++)
            //    Console.WriteLine("Move " + legalMoves[i] + " has " + moveStats[i].victories + " victories / " + moveStats[i].executions + " executions.");

            return legalMoves[bestMoveFound];

        }



        /// <summary>
        /// Returns the move found to have highest win-rate after performing the specified number of Monte-Carlo simulations on the input game.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="simulations">The number of simulations to perform.</param>
        public static TMove Search(TGame game, int simulations)
        {
            // hoist all declarations out of the main loop for performance
            TPlayer aiPlayer = game.CurrentPlayer;
            List<TMove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = JaggedArray.Create(count, new MoveStats());
            int moveIndex;
            TGame copy;
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
            double bestScoreFound = 0f;
            for (int i = 0; i < count; i++)
            {
                double score = moveStats[i].Score();
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
            public int executions;
            public int victories;

            public double Score()
            {
                double vics = victories;
                double execs = executions;
                return vics / execs;
            }
        }
    }
}
