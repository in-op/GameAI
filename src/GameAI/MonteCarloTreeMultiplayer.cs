using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAI
{
    public static class MonteCarloTreeMultiplayer
    {
        public interface Game
        {
            /// <summary>
            /// Returns the current player, represented as an int.
            /// All n players are represented as a number 0,..,n - 1.
            /// </summary>
            /// <returns></returns>
            int GetCurrentPlayer();
            Game DeepCopy();
            /// <summary>
            /// Perform the specified transition. Implementations
            /// must update the hash value.
            /// </summary>
            /// <param name="t"></param>
            void DoMove(Transition t);
            /// <summary>
            /// Perform any random move. To optimize this method,
            /// omit the use and update of the hash value.
            /// </summary>
            void DoRandomMove();
            bool IsGameOver();
            /// <summary>
            /// Returns an array, indexed by numeric player representations,
            /// that specifies, for that player, a win (+1), loss (-1), or tie (0).
            /// </summary>
            /// <returns></returns>
            int[] GetOutcome();
            List<Transition> GetLegalTransitions();
            long GetHash();
        }

        public class Transition
        {
            public Move move;
            public long hash;

            public Transition(Move move, long hash)
            {
                this.move = move;
                this.hash = hash;
            }
        }

        public interface Move { }
    }
}
