using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Interfaces
{
    public interface IChatDispatcher
    {
        bool SendPM(string Username, string message);
        void Join(string room);
    }
}
