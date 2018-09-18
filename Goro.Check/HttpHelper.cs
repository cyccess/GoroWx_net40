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

        public static T Post<T>(string requestUrl, IDictionary<string, string> parameters)
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

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

        /// <summary>
        /// POST json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static string Post(string url, string jsonData)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.ContentType = "text/xml";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;

            byte[] btBodys = Encoding.UTF8.GetBytes(jsonData);
            httpWebRequest.ContentLength = btBodys.Length;
            httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();

            httpWebResponse.Close();
            streamReader.Close();
            httpWebRequest.Abort();
            httpWebResponse.Close();

            return responseContent;
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
            try
            {
                LoggerHelper.Info("requestUrl:" + requestUrl);
                var responseJson = GetString(requestUrl);
                //序列化
                return JsonHelper.Deserialize<T>(responseJson);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            //为了通过证书验证，总是返回true
            return true;
        }
    }
}
