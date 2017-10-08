using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;

namespace TestSenderApplication
{
    public class TestCollector : IPipeCollector
    {
        public DataNode CollectPipeData()
        {
            var data = new DataNode();
            data.Data = new PipeData("This is my test message that I am sending over.");
            return data;
        }
    }
}
