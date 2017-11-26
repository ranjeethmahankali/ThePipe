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
            var data = new DataNode(new PipeString("This is my test message that I am sending over."));
            data.AddChild(new DataNode(new PipeNumber(2.56))); ;
            data.ChildrenList[0].AddChild(new DataNode(new PipeInteger(2)));
            data.ChildrenList[0].AddChild(new DataNode(new PipeInteger(3)));
            data.AddChild(new DataNode(new PipeNumber(3.14)));
            data.ChildrenList[1].AddChild(new DataNode(new PipeString("Does this work ?")));
            return data;
        }
    }
}
