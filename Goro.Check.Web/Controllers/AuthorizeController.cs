using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goro.Check.Service;
using System.Web.Mvc;

namespace Goro.Check.Web.Controllers
{
    public class AuthorizeController : Controller
    {
        public ActionResult Index()
        {
            string reurl = Server.UrlEncode(WebConfig.WebHost + "/Authorize/WxRedirect");

            string url = "https://" + "open.weixin.qq.com/connect/oauth2/authorize?appid=" + WebConfig.CorpID + "&redirect_uri=" + reurl + "&response_type=code&scope=snsapi_base&agentid=" + WebConfig.AgentId + "&state=STATE#wechat_redirect";

            return Redirect(url);
        }


        public ActionResult WxRedirect(string code)
        {
            LoggerHelper.Info("code:" + code);
         
            string access_token = WechatService.GetWrokAccessToken();

            string url = "https://" + "qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token=" + access_token + "&code=" + code;
            var token = HttpHelper.Get<WechatUserInfo>(url);

            LoggerHelper.Info("UserId:" + token.UserId);
            //Cache.CacheService.Set(token.UserId, token);

            string redirectUrl = "/#/?openId=" + token.UserId;

            return Redirect(redirectUrl);
        }


        public ActionResult Index1()
        {
            string reurl = Server.UrlEncode(WebConfig.WebHost + "/Authorize/WxRedirect");
            string url = "https://" + "open.weixin.qq.com/connect/oauth2/authorize?appid=" + WebConfig.APPID + "&redirect_uri=" + reurl + "&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect";
            return Redirect(url);
        }

        
        public ActionResult WxRedirect1(string code)
        {
            LoggerHelper.Info("微信登录code:" + code);

            string url = "https://" + "api.weixin.qq.com/sns/oauth2/access_token?appid=" + WebConfig.APPID + "&secret=" + WebConfig.APPSECRET + "&code=" + code + "&grant_type=authorization_code";

            var token = HttpHelper.Get<WechatToken>(url);

            LoggerHelper.Info("openid:" + token.openid);
            Cache.CacheService.Set(token.openid, token);

            string redirectUrl = "/#/?openId=" + token.openid;

            return Redirect(redirectUrl);
        }



    }
}