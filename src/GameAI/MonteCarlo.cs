using CSharpExtras;
using System;
using System.Collections.Generic;

namespace GameAI
{
    public static class MonteCarlo
    {
        public interface Move { }

        public interface Game
        {
            List<Move> GetLegalMoves();
            void DoMove(Move m);
            bool IsGameOver();
            bool IsWinner(int player);
            Game Copy();
            int GetCurrentPlayer();
        }




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
