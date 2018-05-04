using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class RevitPipeSender : IExternalCommand, IPipeCollector
    {
        private static List<GeometryObject> _selectedObjects = new List<GeometryObject>();

        public DataNode CollectPipeData()
        {
            DataNode node = new DataNode();
            foreach(var geom in _selectedObjects)
            {
                node.AddChild(new DataNode(PipeForRevit.ConvertToPipe(geom)));
            }
            return node;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                string pipeId = PipeForRevit.PipeIdentifier;
                UIApplication uiApp = commandData.Application;
                Document doc = uiApp.ActiveUIDocument.Document;
                PipeForRevit.ActiveDocument = uiApp.ActiveUIDocument.Document;
                Selection sel = uiApp.ActiveUIDocument.Selection;

                List<Reference> picked = sel.PickObjects(ObjectType.Edge, "Select the curves to send through the pipe").ToList();
                _selectedObjects = new List<GeometryObject>();
                foreach (var objRef in picked)
                {
                    Edge edge = (Edge)doc.GetElement(objRef).GetGeometryObjectFromReference(objRef);
                    _selectedObjects.Add(edge.AsCurve());
                }

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
                pipe.SetCollector(this);
                pipe.Update();

                return Result.Succeeded;
            }
            catch(Exception e)
            {
                RevitPipeUtil.ShowMessage("Error", "The following error occured. Aborting operation.", e.Message);
                return Result.Failed;
            }
        }
    }
}
