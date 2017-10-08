using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.MemoryMappedFiles;
using PipeDataModel.DataTree;

namespace PipeDataModel.Pipe
{
    public class MemoryMappedPipe : Pipe
    {
        #region-fields
        private string _name;
        #endregion

        #region-properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        #endregion

        #region-constructors
        public MemoryMappedPipe(string name)
        {
            _name = name;
        }
        #endregion

        #region-base class implementation
        protected override void PushData(DataNode data)
        {
            //incomplete
            throw new NotImplementedException();
        }

        protected override DataNode PullData()
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}
