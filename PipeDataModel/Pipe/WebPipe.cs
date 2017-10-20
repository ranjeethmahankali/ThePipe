using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Pipe
{
    class WebPipe : Pipe
    {
        protected override DataNode PullData()
        {
            //ping the url and get the response
            //deserialize response into DataNode
            //return the DataNode
            //incomplete
            throw new NotImplementedException();
        }

        protected override void PushData(DataNode data)
        {
            //serialize the dataNode to a string
            //send it to the url
            //incomplete
            throw new NotImplementedException();
        }
    }
}
