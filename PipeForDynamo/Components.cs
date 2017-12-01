using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using PipeDataModel.DataTree;
using PipeDataModel.Types;
using PipeDataModel.Pipe;
using Autodesk.DesignScript.Geometry;

namespace PipeForDynamo
{
    public class DynamoPipeComponents
    {
        public static Converters.DynamoPipeConverter _converter = new Converters.DynamoPipeConverter();

        public static object PullFromPipe(string pipeIdentifier)
        {
            DynamoPipeReceiver receiver = new DynamoPipeReceiver(pipeIdentifier, _converter);
            receiver.Update();
            return receiver.Data;
        }

        public static void PushToPipe(string pipeIdentifier, object data)
        {
            DynamoPipeSender sender = new DynamoPipeSender(pipeIdentifier, _converter);
            sender.Data = data;
            sender.Update();
        }
    }
}
