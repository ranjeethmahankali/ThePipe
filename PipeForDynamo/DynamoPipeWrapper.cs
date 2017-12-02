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
    public abstract class DynamoPipeWrapper
    {
        private object _data;
        protected Pipe _pipe;
        protected DynamoPipeConverter _converter;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public DynamoPipeWrapper(string pipeIdentifier, DynamoPipeConverter converter)
        {
            Action finisher = () => {
                _pipe.ClosePipe();
                _pipe = null;
            };
            if (_pipe != null)
            {
                finisher.Invoke();
            }

            if (PipeDataUtil.IsValidUrl(pipeIdentifier)) { _pipe = new MyWebPipe(pipeIdentifier, finisher); }
            else { _pipe = new LocalNamedPipe(pipeIdentifier, finisher); }

            _converter = converter;
        }

        public void Update()
        {
            _pipe.Update();
        }
    }
}
