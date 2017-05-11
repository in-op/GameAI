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
            int Score(int player);
            int GetCurrentPlayer();
        }
        public interface Move { }

        public static Move Search(Game game)
        {
            int player = game.GetCurrentPlayer();
            int bestScore = int.MinValue;
            Move bestMove = null;
            int score;
            foreach (Move move in game.GetLegalMoves())
            {
                score = -NegaMax(game.DoMove(move), player);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                game.UndoMove();
            }
            return bestMove;
        }

        private static int NegaMax(Game game, int player)
        {
            if (game.IsGameOver())
                return game.Score(game.GetCurrentPlayer());

            int bestScore = int.MinValue;
            int score;
            foreach (Move move in game.GetLegalMoves())
            {
                score = -NegaMax(game.DoMove(move), player);
                if (score > bestScore) bestScore = score;
                game.UndoMove();
            }
            return bestScore;
        }
    }
}
