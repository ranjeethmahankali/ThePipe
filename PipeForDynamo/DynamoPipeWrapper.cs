using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using PipeDataModel.Utils;
using PipeDataModel.Pipe;
using PipeForDynamo.Converters;
using dg = Autodesk.DesignScript.Geometry;

namespace PipeForDynamo
{
    internal abstract class DynamoPipeWrapper
    {
        private object _data;
        protected Pipe _pipe;
        protected DynamoPipeConverter _converter;
        /// <summary>
        /// Used to capture error messages that need to be reported to the user
        /// </summary>
        protected string _message = "";

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public string Message { get => _message; }

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

        public bool Update()
        {
            try
            {
                _pipe.Update();
                return true;
            }
            catch(Exception e)
            {
                _message = e.Message;
                return false;
            }
        }

        public void Close()
        {
            _pipe.ClosePipe();
        }
    }
}
