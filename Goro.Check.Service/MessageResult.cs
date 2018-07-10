using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class MessageResult
    {
        public string errcode { get; set; }

        public string errmsg { get; set; }

        public string invaliduser { get; set; }
    }
}
