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
        private static string PIPE_NAME = "test_pipe";
        static void Main(string[] args)
        {
            var senderPipe = new LocalNamedPipe(PIPE_NAME);
            senderPipe.SetCollector(new TestCollector());
            Console.WriteLine("Ready to serve the data.");
            senderPipe.Update();
            Console.WriteLine("The data was received and extracted");
            //Console.ReadKey();
        }
    }
}
