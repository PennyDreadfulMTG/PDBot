using Gatherling.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherling
{
    public interface IPasskeyProvider
    {
        ServerSettings GetServer(string host);
    }

    internal class DefaultPasskeyProvider : IPasskeyProvider
    {
        public ServerSettings GetServer(string host)
        {
            return null;
        }
    }
}
