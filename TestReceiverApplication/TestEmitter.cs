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
            Console.WriteLine("=========================\nThe following is the data fromt the pipe:");
            Console.WriteLine(data.Data.ToString());
        }
    }
}
