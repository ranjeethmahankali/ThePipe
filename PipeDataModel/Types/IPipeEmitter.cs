using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Types
{
    public interface IPipeEmitter
    {
        void EmitPipeData(DataNode data);
        object ConvertFromPipe(IPipeMemberType pipeObj);
    }
}
