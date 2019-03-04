using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gatherling
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieAwareWebClient(CookieContainer container) : base()
        {
            m_container = container;
        }
        private readonly CookieContainer m_container;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest webRequest)
            {
                webRequest.CookieContainer = m_container;
            }
            return request;
        }
    }
}
