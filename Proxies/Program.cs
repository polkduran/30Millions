using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Proxies
{
    class Program
    {
        #region p1

        private static void StartP1()
        {

            Func<HtmlElement, Tuple<bool, string[]>> tryGetProxies = tr => {

                var infos = new string[4];
                var tds = tr.GetElementsByTagName("td");
                if (tds.Count != 4)
                    return Tuple.Create(false,infos);
                
                for (int i = 0; i < 4; i++)
                {
                    infos[i] = tds[i].InnerText;
                }
                return Tuple.Create(true,infos);
            
            };

            var proxies = new List<string>();

            var wbThread = new Thread(() =>
            {
                var page = 1;
                var pageLimit = 31;
                var urlFormat = "http://samair.ru/proxy/proxy-{0:0#}.htm";
                var wb = new WebBrowser();
                wb.Navigating += (s, e) => { Console.WriteLine(string.Format("---- navigating {0}----", page)); };
                wb.DocumentCompleted += (s, e) =>
                {
                    Console.WriteLine("---- completed ----");
                    var trs = wb.Document.GetElementById("proxylist").GetElementsByTagName("tr");
                    int i = 0;
                    for (; i < trs.Count; i++)
                    {
                        var infos = tryGetProxies(trs[i]);
                        if (infos.Item1)
                        {
                            proxies.Add(infos.Item2[0]);
                        }
                    }
                    proxies.Add(string.Format("page {0}", page));
                    Console.WriteLine(string.Format("{0} found", i));
                    ++page;
                    if (page > pageLimit)
                    {
                        Application.ExitThread();
                    }
                    else
                    {
                        wb.Navigate(string.Format(urlFormat, page));
                    }
                };
                wb.Navigate(string.Format(urlFormat, page));
                Application.Run();
            });

            wbThread.SetApartmentState(ApartmentState.STA);
            wbThread.Start();

            wbThread.Join(TimeSpan.FromSeconds(20));
            File.WriteAllLines(@"D:\MyData\Projects\Millions\Proxies\Proxies\1.txt", proxies);
            Console.WriteLine("end");

        }

        #endregion


        private static void P2()
        {
            var reg = new Regex(@"<li\s+class=""proxy""[^>]*>(?<proxy>[^<]+)</li>\s*<li\s+class=""https""[^>]*>(?:<strong>)?(?<http>[^<]+)<");
            Func<int, List<string>> doRequest = page => {
                var url = string.Format("http://proxy-list.org/english/index.php?p={0}", page);
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                var response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    var matches = from Match match in reg.Matches(content)
                                  let proxy = match.Groups["proxy"].Value
                                  where !string.IsNullOrEmpty(proxy)
                                  let http = match.Groups["http"].Value
                                  where !string.IsNullOrEmpty(http)
                                  select http.ToLower()+"://" + proxy;
                    return matches.ToList();
                }
            };

            var results = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                var res = doRequest(i);
                results.AddRange(res);
            }
            File.WriteAllLines(@"D:\MyData\Projects\Millions\Proxies\Proxies\2.txt", results);
        }

        static void Main(string[] args)
        {
            P2();
        }
        
    }
}
