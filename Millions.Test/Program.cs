using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Millions.Test
{
    class Program
    {

        static void Main(string[] args)
        {
            var settings = new VoterSettings { ProxyAddress = "http://37.187.97.36", ProxyPort = "3128" };
            var result = Voter.VoteOnce(settings);
            Console.WriteLine(result.Ok);

            if (result.Ok)
            {
                Console.WriteLine(result.VoteResponse);
            }

            Console.WriteLine(string.Join(Environment.NewLine, result.Logs));

            Console.WriteLine("end");
            Console.ReadLine();
        }
    }
}
