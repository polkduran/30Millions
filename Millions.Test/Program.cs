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
        static void Main(string[] args)
        {
            var result = Voter.VoteOnce(null);
            Console.WriteLine(result.Ok);

            if (result.Ok)
            {
                Console.WriteLine(result.VoteResponse);
            }

            Console.WriteLine("end");
            Console.ReadLine();
        }
    }
}
