using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class MessageJob : IJob
    {
        public void Execute()
        {
            ApiService.SendNoticeToGM();
        }
    }
}
