using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Pipe
{
    public class WebPipe : Pipe
    {
        #region-fields
        private string _url;
        private Action _callBack = null;
        #endregion

        #region-constructors
        public WebPipe(string url)
        {
            _url = url;
        }
        public WebPipe(string url, Action callBack):this(url)
        {
            _callBack = callBack;
        }
        #endregion

        #region-base class implementation
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

        public override void ClosePipe()
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}
