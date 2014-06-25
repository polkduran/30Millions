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
            var lines = from line in File.ReadAllLines(@"D:\Mine\Repos\30Millions\Proxies\proxies.txt")
                        let match = Regex.Match(line, @"(?<ip>\d+(?:\.\d+){3})(?::(?<port>\d+))?")
                        where match.Success
                        select new VoterSettings { ProxyAddress = "http://" + match.Groups["ip"].Value, ProxyPort = match.Groups["port"].Value };
            return lines.ToList();
        }


        static void Main(string[] args)
        {
            var randTime = new Random();
            var i = 0;
            foreach (var settings in GenerateSettings())
            {
                var log = new List<string>();
                log.Add("#-#-#");   
                var result = Voter.VoteOnce(settings);
                log.Add(result.Ok.ToString());
                
                if (result.Ok)
                {
                    log.Add(result.VoteResponse);
                }

                log.Add(string.Join(Environment.NewLine, result.Logs));
                log.Add("#-#-#");   
                File.AppendAllLines(string.Format("Logs\\Log_{0}.txt",DateTime.Now.ToString("dd_hh_mm")),log);
                var towait = randTime.Next(30, 300);
                Thread.Sleep(TimeSpan.FromSeconds(towait));
                i++;
                Console.WriteLine(i);
            }

            
            Console.WriteLine("end");
            Console.ReadLine();
        }
    }
}
