using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;
using PipeForDynamo.Converters;

namespace PipeForDynamo
{
    internal class DynamoPipeReceiver : DynamoPipeWrapper, IPipeEmitter
    {
        internal static Dictionary<string, DynamoPipeReceiver> _receivers = new Dictionary<string, DynamoPipeReceiver>();

        private DynamoPipeReceiver(string pipeIdentifier, DynamoPipeConverter converter) : base(pipeIdentifier, converter)
        {
            _pipe.SetEmitter(this);
        }

        public void EmitPipeData(DataNode data)
        {
            // bail out if nothing is received
            if (data == null) { return; }
            List<object> objs = new List<object>();
            foreach (var child in data.ChildrenList)
            {
                objs.Add(_converter.FromPipe<object, IPipeMemberType>(child.Data));
            }
            Data = objs.Count == 1 ? objs[0] : objs;
        }

        internal static DynamoPipeReceiver GetReceiver(string name, DynamoPipeConverter converter)
        {
            if (_receivers.ContainsKey(name)) { return _receivers[name]; }
            else
            {
                var rec = new DynamoPipeReceiver(name, converter);
                _receivers.Add(name, rec);
                return rec;
            }
        }
    }
}
