using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAI.GameInterfaces
{
    public interface IRollout
    {
        /// <summary>
        /// Play the game until it ends.
        /// Omit the use and update of
        /// the hash value for performance.
        /// </summary>
        void Rollout();
    }
}
