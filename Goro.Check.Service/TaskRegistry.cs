using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class TaskRegistry : Registry
    {
        public TaskRegistry()
        {
            Schedule<MessageJob>().ToRunNow().AndEvery(10).Seconds();
        }
    }
}
