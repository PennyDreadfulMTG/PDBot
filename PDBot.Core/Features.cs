﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    class Features
    {
        /// <summary>
        /// There is currently a bug with MTGO that gets the names wrong.
        /// We don't want to tell people incorrect results when this happens.
        /// </summary>
        public static readonly bool PublishResults = false;
    }
}