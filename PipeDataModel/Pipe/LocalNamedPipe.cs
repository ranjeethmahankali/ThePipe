using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using PipeDataModel.DataTree;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace PipeDataModel.Pipe
{
    public class LocalNamedPipe : Pipe
    {
        #region-fields
        private string _name;
        private Action _callBack = null;
        private NamedPipeServerStream _pipeServer;
        private NamedPipeClientStream _pipeClient;

        private IAsyncResult _result;
        private bool _isActive = false;
        #endregion

        #region-properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        #endregion

        #region-constructors
        public LocalNamedPipe(string name)
        {
            _name = name;
        }
        public LocalNamedPipe(string name, Action callBack) : this(name)
        {
            _callBack = callBack;
        }
        #endregion

        #region-base class implementation
        protected override void PushData(DataNode data)
        {
            /*
             * if the server is already active trying to send the data and waiting for a listener. We need to stop it before 
             * sending the latest data again. So we check if it is active, create a fake listener and intercept the data to make the
             * pipe close and then we can continue doing our thing and sending the new data
             */
            if (_isActive)
            {
                DataNode oldData = PullData();
                //if the new data and the old data are same then we don't need to resend it so we bail out.
                if (oldData.Equals(data)) { return; }
            }

            _pipeServer = new NamedPipeServerStream(_name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _result = _pipeServer.BeginWaitForConnection(ar => {
                _isActive = false;
                _pipeServer.EndWaitForConnection(ar);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(_pipeServer, data);
                _pipeServer.WaitForPipeDrain();
                _pipeServer.Flush();
                _pipeServer.Close();
                _pipeServer.Dispose();
                _callBack.Invoke();
            }, _pipeServer);
            _isActive = true;
        }

        protected override DataNode PullData()
        {
            if(_pipeClient == null)
            {
                _pipeClient = new NamedPipeClientStream(".", _name, PipeDirection.In, PipeOptions.Asynchronous);
                _pipeClient.Connect();
            }
            //if (!pipeClient.IsConnected) { return null; }
            BinaryFormatter bf = new BinaryFormatter();
            object received = bf.Deserialize(_pipeClient);
            _pipeClient.Close();
            _pipeClient = null;
            return (DataNode)received;
        }
        #endregion

        #region-methods
        public override void Update()
        {
            base.Update();
        }
        #endregion
    }
}
