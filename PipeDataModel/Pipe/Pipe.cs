using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;

namespace PipeDataModel.Pipe
{
    public abstract class Pipe
    {
        /*
         * This is just an abstract class for pipe. This class can be inherited by any type of pipe class.
         * Some pipe classes might transmit the data by saving it to disk and retrieving it on the other side,
         * or maybe send it over to a server and then pull it from the server, Whatever, these details depend
         * on the exact implementation of the class that will inherit this class.
         */
        #region-fields
        private IPipeCollector _collector = null;
        private IPipeEmitter _emitter = null;
        #endregion

        #region-constructors
        #endregion

        #region-methods
        public void SetCollector(IPipeCollector collector)
        {
            _collector = collector;
        }
        public void SetEmitter(IPipeEmitter emitter)
        {
            _emitter = emitter;
        }

        /*
         * As the methods and fields indicate, all pipes have a collector and an emitter that can be set via
         * the setter methods. The PushData should take the data received from teh collector and
         * push it to a persistent location - be it a file on the disk, or a server via HTTP, whatever.
         * The PullData method should retrieve the data from the persistent location associated with this pipe,
         * and return it.
         * 
         * Since the exact implementations of these methods might change from one type of pipe to another,
         * these are declared as abstract. The Update() method uses both methods to make the pipe do it's thing.
         * And the pipe classes that inherit this only have to implement the logic for pushing and pulling the
         * data.
         */
        protected abstract void PushData(DataNode data);
        protected abstract DataNode PullData();
        public abstract void ClosePipe();

        public virtual void Update()
        {
            if (_collector != null) { PushData(_collector.CollectPipeData()); }
            if(_emitter != null) { _emitter.EmitPipeData(PullData()); }
        }
        #endregion
    }
}
