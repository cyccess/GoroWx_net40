using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Goro.Check
{
    public abstract class HttpHelper
    {

        public static T Post<T>(string requestUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "POST";
            var response = request.GetResponse();
            string responseJson = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseJson = reader.ReadToEnd();
            }

            //序列化
            return JsonHelper.Deserialize<T>(responseJson);
        }

        public static string GetString(string requestUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";
            var response = request.GetResponse();
            string responseJson = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseJson = reader.ReadToEnd();
            }

            return responseJson;
        }

        public static T Get<T>(string requestUrl)
        {
            var responseJson = GetString(requestUrl);
            //序列化
            return JsonHelper.Deserialize<T>(responseJson);
        }

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            //为了通过证书验证，总是返回true
            return true;
        }
    }
}
