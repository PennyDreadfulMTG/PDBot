using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Interfaces
{
    public interface IGameList
    {
        IEnumerable<IMatch> ActiveMatches { get; }
    }
}
