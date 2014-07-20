using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Millions.Test
{
    class Program
    {

        private static IReadOnlyCollection<VoterSettings> GenerateSettings()
        {
            var lines = from file in Directory.GetFiles(@"D:\MyData\Projects\Millions\Proxies\Proxies", "*.txt", SearchOption.AllDirectories)
                        from line in File.ReadAllLines(file)
                        let match = Regex.Match(line, @"(?<ip>\d+(?:\.\d+){3})(?::(?<port>\d+))?")
                        where match.Success
                        let server = match.Groups["ip"].Value
                        let port = match.Groups["port"].Value
                        let scheme = server.StartsWith("http") ? "" : "http://"
                        select new VoterSettings { ProxyAddress = scheme + server, ProxyPort = port };
            return lines.Distinct().ToList();
        }


        static void Main(string[] args)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            var randTime = new Random();
            var i = 0;
            var oks = 0;
            foreach (var settings in GenerateSettings())
            {
                var log = new List<string>();
                log.Add("#-#-#");
                var result = Voter.VoteOnce(settings);
                log.Add(result.Ok.ToString());

                if (result.Ok)
                {
                    log.Add(result.VoteResponse);
                    if (result.VoteResponse.StartsWith("{\"message\":\"\",\"nbvotes\":"))
                    {
                        oks++;
                    }
                }

                log.Add(string.Join(Environment.NewLine, result.Logs));
                log.Add("#-#-#");
                File.AppendAllLines(string.Format("Logs\\Log_{0}.txt", DateTime.Now.ToString("dd_hh_mm")), log);
                var towait = randTime.Next(5, 20);
                if (i % 50 == 0)
                {
                    towait += randTime.Next(300, 600);
                }
                Thread.Sleep(TimeSpan.FromSeconds(towait));
                i++;
                Console.WriteLine("{0}/{1}", oks, i);
            }


            Console.WriteLine("end");
            Console.WriteLine(string.Format("Oks : {0}", oks));

            Console.ReadLine();
        }
    }
}
