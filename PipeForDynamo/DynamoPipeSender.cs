using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;
using PipeDataModel.Pipe;
using PipeDataModel.Utils;
using PipeForDynamo.Converters;

namespace PipeForDynamo
{
    internal class DynamoPipeSender : DynamoPipeWrapper, IPipeCollector
    {
        internal static Dictionary<string, DynamoPipeSender> _senders = new Dictionary<string, DynamoPipeSender>();

        private DynamoPipeSender(string pipeIdentifier, DynamoPipeConverter converter) : base(pipeIdentifier, converter)
        {
            _pipe.SetCollector(this);
        }

        public DataNode CollectPipeData()
        {
            //convert the _data object and return it
            if(Data == null) { return null; }
            DataNode node = new DataNode();
            node.AddChild(new DataNode(_converter.ToPipe<object, IPipeMemberType>(Data)));
            return node;
        }

        internal static DynamoPipeSender GetSender(string name, DynamoPipeConverter converter)
        {
            if (_senders.ContainsKey(name)) { return _senders[name]; }
            else
            {
                var sender = new DynamoPipeSender(name, converter);
                _senders.Add(name, sender);
                return sender;
            }
        }
    }
}
