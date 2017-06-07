using System.Collections.Generic;

namespace GameAI.GameInterfaces
{
    public interface ILegalTransitions<TTransition>
    {
        List<TTransition> GetLegalTransitions();
    }
}
