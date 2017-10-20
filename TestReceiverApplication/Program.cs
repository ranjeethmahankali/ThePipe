using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Pipe;

namespace TestReceiverApplication
{
    class Program
    {
        private static string PIPE_NAME = "first";
        static void Main(string[] args)
        {
            var receiverPipe = new LocalNamedPipe(PIPE_NAME);
            receiverPipe.SetEmitter(new TestEmitter());

            receiverPipe.Update();
            Console.ReadKey();
        }
    }
}
