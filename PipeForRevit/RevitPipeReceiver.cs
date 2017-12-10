using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using PipeDataModel.Types;
using PipeDataModel.Utils;
using PipeDataModel.Pipe;
using PipeForRevit.Utils;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.DB.ExternalService;
using PipeDataModel.DataTree;

namespace PipeForRevit
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RevitPipeReceiver : IExternalCommand, IPipeEmitter
    {
        private List<GeometryObject> _receivedObjects = new List<GeometryObject>();
        public void EmitPipeData(DataNode data)
        {
            _receivedObjects = new List<GeometryObject>();
            if(data == null) { return; }
            foreach(var child in data.ChildrenList)
            {
                _receivedObjects.Add(PipeForRevit.ConvertFromPipe(child.Data));
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string pipeId = PipeForRevit.PipeIdentifier;
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            Selection sel = uiApp.ActiveUIDocument.Selection;

            Pipe pipe = null;
            Action callBack = () => {
                if (pipe != null)
                {
                    pipe.ClosePipe();
                }
                RevitPipeUtil.ShowMessage("Success", "Pushed data to the pipe.");
            };
            if (PipeDataUtil.IsValidUrl(pipeId))
            {
                pipe = new MyWebPipe(pipeId, callBack);
            }
            else
            {
                pipe = new LocalNamedPipe(pipeId, callBack);
            }
            pipe.SetEmitter(this);
            pipe.Update();

            Transaction trans = new Transaction(doc);
            trans.Start("pipe_pull");
            foreach (var geom in _receivedObjects)
            {
                if (typeof(Curve).IsAssignableFrom(geom.GetType()))
                {
                    RevitPipeUtil.AddCurveToDocument(ref doc, (Curve)geom);
                }
            }
            trans.Commit();

            return Result.Succeeded;
        }
    }
}
