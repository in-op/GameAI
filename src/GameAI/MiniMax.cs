using System.Collections.Generic;

namespace GameAI
{
    public static class MiniMax
    {
        public interface Game
        {
            List<Move> GetLegalMoves();
            Game DoMove(Move move);
            void UndoMove();
            bool IsGameOver();
            int CurrentPlayersScore();
        }
        public interface Move { }

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
