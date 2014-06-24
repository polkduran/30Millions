using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Millions.Test
{
    class Program
    {
        static void Write(string mesage)
        {

        }




        static void Main(string[] args)
        {
            Do();

            Console.WriteLine("end");
            Console.ReadLine();
        }


        static void SetHeaders(HttpWebRequest request)
        {
            request.KeepAlive = true;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,fr;q=0.6,es;q=0.4,zh-CN;q=0.2,zh;q=0.2,de;q=0.2");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
        }

        static void SetProxy(HttpWebRequest request)
        {
            var proxyHost = "";
            var proxyPort = "";
            var proxyAddress = string.Format("{0}{1}", proxyHost, string.IsNullOrEmpty(proxyPort) ? "" : ":" + proxyPort);

            var proxy = new WebProxy();
            proxy.Address = new Uri(proxyAddress);

            request.Proxy = proxy;
        }

        static void Do()
        {
            var startUrl = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.html";

            var cookieContainer = new CookieContainer();
            var request = (HttpWebRequest)HttpWebRequest.Create(startUrl);
            request.CookieContainer = cookieContainer;
            request.Method = "GET";
            SetHeaders(request);
            //SetProxy(request);

            PrintCookies(request);

            var response = request.GetResponse();
            PrintResponse(response);

            var voteUrl = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.php";
            request = (HttpWebRequest)HttpWebRequest.Create(voteUrl);
            request.CookieContainer = cookieContainer;
            request.Method = "POST";
            SetHeaders(request);
            //
            var postData = "act=vote&uid=1465";
            var data = Encoding.ASCII.GetBytes(postData);
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers.Add("Origin", "http://www.30millionsdamis.fr");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Referer = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.html";
            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
        
            //

            
            PrintCookies(request);

            response = request.GetResponse();
            PrintResponse(response);

        }

        static void PrintCookies(HttpWebRequest request)
        {
            Console.WriteLine("Cookies:");
            if (request.CookieContainer == null || request.CookieContainer.Count == 0)
            {
                Console.WriteLine("no cookies");
            }
            else
            {
                Console.WriteLine(string.Format("Cookies: {0}.", request.CookieContainer.Count));
            }
        }

        static void PrintHeaders(WebHeaderCollection headers)
        {
            if (headers == null || headers.Count == 0)
            {
                Console.WriteLine("No headers");
            }
            else
            {
                foreach (var header in headers)
                {
                    Console.WriteLine(string.Format("{0} : {1}", header, headers[header.ToString()]));
                }
            }
        }

        static void PrintResponse(WebResponse response)
        {
            PrintHeaders(response.Headers);
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var content = reader.ReadToEnd();
                Console.WriteLine(content.Substring(0, 100));
            }
        }

    }
}
