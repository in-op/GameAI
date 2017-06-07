using System.Collections.Generic;
using SystemExtensions.Copying;

namespace GameAI.MonteCarlo
{
    public static class UCB1TreeMultiplayer
    {
        public interface IGame<TMove> :
            ICopyable<IGame<TMove>>,
            IInt64Hash,
            IGameOver
        {
            /// <summary>
            /// Returns the current player, represented as an int.
            /// All n players are represented as a number 0,..,n - 1.
            /// </summary>
            int GetCurrentPlayer();
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
            /// <summary>
            /// Returns an array, indexed by numeric player representations,
            /// that specifies, for that player, a win (+1), loss (-1), or tie (0).
            /// </summary>
            int[] GetOutcome();
            List<Transition<TMove>> GetLegalTransitions();
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
