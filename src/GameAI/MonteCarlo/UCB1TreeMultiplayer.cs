using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAI.MonteCarlo
{
    public static class UCB1TreeMultiplayer
    {
        public interface IGame<TMove>
        {
            /// <summary>
            /// Returns the current player, represented as an int.
            /// All n players are represented as a number 0,..,n - 1.
            /// </summary>
            int GetCurrentPlayer();
            /// <summary>
            /// Returns a deep copy of the game.
            /// </summary>
            IGame<TMove> DeepCopy();
            /// <summary>
            /// Perform the specified transition. Implementations
            /// must update the hash value.
            /// </summary>
            void DoMove(Transition<TMove> t);
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
            int[] GetOutcome();
            List<Transition<TMove>> GetLegalTransitions();
            long GetHash();
        }

        public class Transition<TMove>
        {
            public TMove move;
            public long hash;

            public Transition(TMove move, long hash)
            {
                this.move = move;
                this.hash = hash;
            }

            public override string ToString()
            {
                return "Move: " + move.ToString() + ". Hash: " + hash.ToString();
            }
        }
        
    }
}
