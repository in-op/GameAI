using System;
using System.Collections.Generic;
using SystemExtensions;
using SystemExtensions.Random;
using SystemExtensions.Copying;
using System.Threading;

namespace GameAI.MonteCarlo
{
    public static class RandomSimulationTest<tgame, tmove>
        where tgame : RandomSimulationTest<tgame, tmove>.game
    {

        public interface game :
            ICopyable<tgame>,
            IDoMove<tmove>,
            IGameOver,
            ICurrentPlayer,
            ILegalMoves<tmove>
        {
            bool IsWinner(int player);
        }


        public static tmove parallelsearch(tgame game, int simulations)
        {
            int aiPlayer = game.CurrentPlayer;
            List<tmove> legalMoves = game.GetLegalMoves();
            int count = legalMoves.Count;
            MoveStats[] moveStats = JaggedArray.Create(count, new MoveStats());

            ParallelNET35.Parallel.For(0, simulations,

                () => { return ThreadLocalRandom.Instance; },

                (i, loop, localRandom) =>
                {
                    int moveIndex = localRandom.Next(0, count);
                    tgame copy = game.DeepCopy();
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
