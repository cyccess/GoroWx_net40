using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goro.Check.Service
{
    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            // 立即执行每1小时一次的计划任务
            //Schedule<TokenJob>().ToRunNow().AndEvery(60).Minutes();

            Schedule<MessageJob>().NonReentrant().ToRunNow().AndEvery(10).Seconds();

            LoggerHelper.Info("消息定时服务已启动");
        }
    }
}
