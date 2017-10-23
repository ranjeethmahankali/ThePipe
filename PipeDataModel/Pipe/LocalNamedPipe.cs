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
        private static int THREAD_TIMEOUT_MILLIS = 60000;

        private string _name;
        private Action _callBack = null;
        private Thread _pipeThread;
        private Thread _timerThread;
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
            _timerThread = new Thread(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    if (watch.ElapsedMilliseconds >= THREAD_TIMEOUT_MILLIS)
                    {
                        watch.Stop();
                        watch.Reset();
                        if (_pipeThread != null) { _pipeThread.Abort(); }
                        Debug.WriteLine("Stopping the server thread due to timeout.");
                        break;
                    }
                }
            });
        }
        public LocalNamedPipe(string name, Action callBack) : this(name)
        {
            _callBack = callBack;
        }
        #endregion

        #region-base class implementation
        protected override void PushData(DataNode data)
        {
            var pipeServer = new NamedPipeServerStream(_name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeServer.WaitForConnection();
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(pipeServer, data);
                pipeServer.WaitForPipeDrain();
            }
            catch(Exception e)
            {
                pipeServer.Close();
                pipeServer = null;
                throw e;                
            }
            pipeServer.Close();
            pipeServer = null;
        }

        protected override DataNode PullData()
        {
            var pipeClient = new NamedPipeClientStream(".", _name, PipeDirection.In, PipeOptions.None);
            pipeClient.Connect();
            BinaryFormatter bf = new BinaryFormatter();
            object received = bf.Deserialize(pipeClient);
            pipeClient.Close();
            pipeClient = null;
            return (DataNode)received;
        }
        #endregion

        #region-methods
        public override void Update()
        {
            _pipeThread = new Thread(() =>
            {
                base.Update();
                if(_callBack != null) { _callBack.Invoke(); }
            });
            //_timerThread.Start();
            _pipeThread.Start();
        }
        #endregion
    }
}
