using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Goro.Check.Service
{
   public class TokenJob : IJob
    {
        public void Execute()
        {
            WechatService.RefreshAccessToken();
        }
    }
}
