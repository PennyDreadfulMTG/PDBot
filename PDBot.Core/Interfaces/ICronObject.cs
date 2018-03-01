using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Interfaces
{
    public interface ICronObject
    {
        Task EveryMinute();
        Task EveryHourAsync();
    }
}
