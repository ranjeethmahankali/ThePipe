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
            var data = new DataNode(new PipeData("This is my test message that I am sending over."));
            data.AddChild(new DataNode(new PipeData(2.56))); ;
            data.ChildrenList[0].AddChild(new DataNode(new PipeData(2)));
            data.ChildrenList[0].AddChild(new DataNode(new PipeData(3)));
            data.AddChild(new DataNode(new PipeData(3.14)));
            data.ChildrenList[1].AddChild(new DataNode(new PipeData("Does this work ?")));
            return data;
        }

        public IPipeMemberType ConvertToPipe(object obj)
        {
            //incomplete
            throw new NotImplementedException();
        }
    }
}
