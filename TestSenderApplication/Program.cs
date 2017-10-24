using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;
using PipeDataModel.Types;

namespace TestSenderApplication
{
    class Program
    {
        private static string PIPE_NAME = "first";
        static void Main(string[] args)
        {
            Action finihser = () =>
            {
                Console.WriteLine("The data was received and extracted");
            };
            var senderPipe = new LocalNamedPipe(PIPE_NAME, finihser);
            senderPipe.SetCollector(new TestCollector());
            Console.WriteLine("Ready to serve the data.");
            senderPipe.UpdateAsync();
            Console.ReadKey();
        }
    }
}
