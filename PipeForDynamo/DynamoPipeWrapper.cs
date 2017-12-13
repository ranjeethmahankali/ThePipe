using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using PipeDataModel.Utils;
using PipeDataModel.Pipe;
using PipeForDynamo.Converters;

namespace PipeForDynamo
{
    internal abstract class DynamoPipeWrapper
    {
        private object _data;
        protected Pipe _pipe;
        protected DynamoPipeConverter _converter;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        internal DynamoPipeWrapper(string pipeIdentifier, DynamoPipeConverter converter)
        {
            if (_pipe != null)
            {
                _pipe.ClosePipe();
                _pipe = null;
            }

            if (PipeDataUtil.IsValidUrl(pipeIdentifier)) { _pipe = new MyWebPipe(pipeIdentifier); }
            else { _pipe = new LocalNamedPipe(pipeIdentifier); }

            _converter = converter;
        }

        public void Update()
        {
            _pipe.Update();
        }
    }
}
