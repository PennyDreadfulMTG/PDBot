using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gatherling.Models;

namespace Gatherling.VersionedApis
{
    internal class V2 : V1
    {
        public V2(ServerSettings settings, CookieContainer cookies) : base(settings, cookies)
        {
        }

        public override int ApiVersion => 2;

        //public override Task<Event[]> GetActiveEventsAsync()
        //{
        //    throw new NotImplementedException();
        //}

        //public override Task<Round> GetCurrentPairings(string eventName)
        //{
        //    throw new NotImplementedException();
        //}

        //public override Event LoadEvent(string name)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
