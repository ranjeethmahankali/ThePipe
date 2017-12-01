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
    public class DynamoPipeSender : DynamoPipeWrapper, IPipeCollector
    {
        public DynamoPipeSender(string pipeIdentifier, DynamoPipeConverter converter) : base(pipeIdentifier, converter)
        {
            _pipe.SetCollector(this);
        }

        public DataNode CollectPipeData()
        {
            //convert the _data object and return it
            return Data == null ? null : new DataNode(_converter.ToPipe<object, IPipeMemberType>(Data));
        }
    }
}
