using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;

namespace TestReceiverApplication
{
    public class TestEmitter : IPipeEmitter
    {
        public void EmitPipeData(DataNode data)
        {
            Console.WriteLine("The following is the data from the pipe\n=========================\n");
            Console.WriteLine(data?.ToString());
        }
    }
}
