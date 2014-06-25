﻿using System;
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
            var wbThread = new Thread(() =>
            {
                var wb = new WebBrowser();
                wb.Navigating += (s, e) => { Console.WriteLine("---- navigating ----"); };
                wb.DocumentCompleted += (s, e) =>
                {
                    Console.WriteLine("---- completed ----");
                    var trs = wb.Document.GetElementById("proxylist").GetElementsByTagName("tr");
                    for (int i = 0; i < trs.Count; i++)
                    {
                        string[] infos;
                        if (TryGetInfos(trs[i], out infos))
                        {
                            Console.WriteLine(string.Join(",", infos));
                        }
                    }

                    Application.ExitThread();
                };
                wb.Navigate("http://samair.ru/proxy/proxy-01.htm");
                Application.Run();
            });

            wbThread.SetApartmentState(ApartmentState.STA);
            wbThread.Start();

            wbThread.Join(TimeSpan.FromSeconds(20));
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
