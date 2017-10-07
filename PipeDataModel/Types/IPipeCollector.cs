using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Types
{
    public interface IPipeCollector
    {
        DataNode CollectPipeData();
    }
}
