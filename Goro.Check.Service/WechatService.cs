﻿using Goro.Check;
using Goro.Check.Cache;
using Goro.Check.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goro.Check.Service
{
    public static class WechatService
    {

        private static object _accessTokenLock = new object();
        /// <summary> 
        /// 将c# DateTime时间格式转换为Unix时间戳格式 
        /// </summary> 
        /// <param name="time">时间</param> 
        /// <returns>long</returns> 
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位     
            return t;
        }

        /// <summary>
        /// 获取公众号 AccessToken
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken()
        {
            var token = CacheService.Get("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                string url = "https://" + "api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + WebConfig.APPID + "&secret=" + WebConfig.APPSECRET;

                var accessToken = HttpHelper.Get<AccessToken>(url);
                token = accessToken.access_token;
                CacheService.Set("AccessToken", token, new TimeSpan(1, 58, 0));
                LoggerHelper.Info("重新获取AccessToken=" + token);
            }

            return token;
        }

        /// <summary>
        /// 获取企业微信 AccessToken
        /// </summary>
        /// <returns></returns>
        public static string GetWrokAccessToken()
        {
            try
            {
                lock (_accessTokenLock)
                {
                    var token = CacheService.Get("AccessToken");
                    if (string.IsNullOrEmpty(token))
                    {
                        RefreshAccessToken();
                        token = CacheService.Get("AccessToken");
                    }

                    return token;
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Info("获取AccessToken出错:" + e);
                return "";
            }
        }


        public static void RefreshAccessToken()
        {
            LoggerHelper.Info("开始刷新AccessToken");
            string url = "https://" + "qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=" + WebConfig.CorpID + "&corpsecret=" + WebConfig.Secret;
            var accessToken = HttpHelper.Get<AccessToken>(url);

            if (accessToken != null)
            {
                LoggerHelper.Info("AccessToken已刷新 code:" + accessToken.errcode);
                if (accessToken.errcode == 0)
                {
                    CacheService.Set("AccessToken", accessToken.access_token, new TimeSpan(1, 59, 0));
                    LoggerHelper.Info("AccessToken已刷新:" + accessToken.access_token);
                }
            }
        }


        public static void Send(string touser, string title, string desc, string url)
        {
            string accessToken = GetWrokAccessToken();

            if (string.IsNullOrEmpty(touser) || accessToken == "") return;

            string requestUrl = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=" + accessToken;
            var now = DateTime.Now.ToString("M月d日");

            var textcard = new
            {
                title,
                //支持使用br标签或者空格来进行换行处理
                description = "<div class=\"gray\">" + now + "</div> <br>" + desc,
                url,
                btntxt = "详情"
            };

            /// todo: 通知我
            touser += "|cyccess";
            LoggerHelper.Info("发送消息:" + touser);

            var postJson = new
            {
                touser,
                toparty = "",
                totag = "",
                msgtype = "textcard",
                agentid = 1000002,
                textcard
            };
            try
            {
                string res = HttpHelper.Post(requestUrl, JsonHelper.Serialize(postJson));
            }
            catch (Exception e)
            {
                LoggerHelper.Info("Send Message Error:" + e);
            }
        }


        public static string ComposeUrl(Dictionary<string, string> paras)
        {
            var queryString = paras.Select(p => p.Key + "=" + p.Value).ToArray();

            string sortedQueryString = string.Join("&", queryString);

            return sortedQueryString;
        }


        /// <summary>
        /// HMAC-SHA1加密算法
        /// </summary>
        /// <param name="str">加密字符串</param>
        /// <returns></returns>
        public static string HmacSha1Sign(string str)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.Default.GetBytes(str));
            string byte2String = null;
            for (int i = 0; i < hash.Length; i++)
            {
                byte2String += hash[i].ToString("x2");
            }
            return byte2String;
        }
    }
}
