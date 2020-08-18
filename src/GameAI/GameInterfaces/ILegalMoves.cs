using System.Collections.Generic;

namespace GameAI.GameInterfaces
{
    public interface ILegalMoves<TMove>
    {
        /// <summary>
        /// Return a list of legal moves for the current gamestate.
        /// </summary>
        List<TMove> GetLegalMoves();
    }
}
