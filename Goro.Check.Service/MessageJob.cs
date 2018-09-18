using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class MessageJob : IJob
    {
        private readonly object _lock = new object();

        public void Execute()
        {
            lock (_lock)
            {
                ApiService.SendNotice();
            }
           
        }
    }
}
