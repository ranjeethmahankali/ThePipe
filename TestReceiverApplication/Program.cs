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
            Action finisher = () =>
            {
                Console.WriteLine("Data transfer finished!.");
            };
            var receiverPipe = new LocalNamedPipe(PIPE_NAME, finisher);
            receiverPipe.SetEmitter(new TestEmitter());

            receiverPipe.UpdateAsync();
            Console.ReadKey();
        }
    }
}
