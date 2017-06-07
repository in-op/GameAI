using System.Collections.Generic;

namespace GameAI
{
    public interface ILegalMoves<TMove>
    {
        /// <summary>
        /// Returns a list of legal moves
        /// for the current gamestate.
        /// </summary>
        List<TMove> GetLegalMoves();
    }
}
