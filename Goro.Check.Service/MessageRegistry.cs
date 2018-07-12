using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class MessageRegistry : Registry
    {
        public MessageRegistry()
        {
            Schedule<MessageJob>().ToRunNow().AndEvery(10).Seconds();
        }
    }
}
