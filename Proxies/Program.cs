using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Proxies
{
    class Program
    {





        static void Main(string[] args)
        {
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
                        string[] infos;
                        if (TryGetInfos(trs[i], out infos))
                        {
                            proxies.Add(infos[0]);
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
            File.WriteAllLines(@"C:\Users\pfernandez\Desktop\b.txt", proxies);
            Console.WriteLine("end");

        }

        static bool TryGetInfos(HtmlElement tr, out string[] infos)
        {
            infos = null;
            var tds = tr.GetElementsByTagName("td");
            if (tds.Count != 4)
                return false;
            infos = new string[4];
            for (int i = 0; i < 4; i++)
            {
                infos[i] = tds[i].InnerText;
            }
            return true;
        }

    }
}
