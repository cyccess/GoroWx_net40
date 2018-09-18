using FluentScheduler;
using Goro.Check.Cache;
using Goro.Check.Service;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Goro.Check.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            WebConfig.ConnectionString = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            WebConfig.APPID = ConfigurationManager.AppSettings["APPID"];
            WebConfig.APPSECRET = ConfigurationManager.AppSettings["APPSECRET"];
            WebConfig.WebHost = ConfigurationManager.AppSettings["WebHost"];

            WebConfig.CorpID = ConfigurationManager.AppSettings["CorpID"];
            WebConfig.AgentId = ConfigurationManager.AppSettings["AgentId"];
            WebConfig.Secret = ConfigurationManager.AppSettings["Secret"];

            WebConfig.JobInterval = ConfigurationManager.AppSettings["JobInterval"];

            CacheService.Init();

            // 定时消息
            JobManager.Initialize(new MyRegistry());
        }

        protected void Application_End()
        {
            LoggerHelper.Info("进程即将被IIS回收");
            LoggerHelper.Info("重新访问一个页面，以唤醒服务");

            //var res = HttpHelper.GetString("http://wx.goro.com.cn/api/test");

            //if (res != "")
            //{
            //    LoggerHelper.Info("唤醒程序成功");
            //}
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Context.Request.FilePath == "/") Context.RewritePath("index.html");
        }
    }
}