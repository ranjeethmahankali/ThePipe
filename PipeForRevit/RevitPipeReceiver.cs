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
        private static List<GeometryObject> _receivedObjects = new List<GeometryObject>();
        private static List<ElementId> _previousIds = new List<ElementId>();
        private static List<Reference> _previousRefs = new List<Reference>();
        private static Document _document;

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
            _document = uiApp.ActiveUIDocument.Document;
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

            if(GeometryTypeMatch())
            {
                bool deleteExisting;
                bool updateGeom = UserDecidedToUpdateGeometry(out deleteExisting);
                if (updateGeom)
                {
                    UpdateGeometry(_receivedObjects);
                }
                else
                {
                    _previousIds = AddObjectsToDocument(_receivedObjects, deleteExisting);
                }
            }
            else
            {
                _previousIds = AddObjectsToDocument(_receivedObjects, false);
            }

            return Result.Succeeded;
        }

        private bool UserDecidedToUpdateGeometry(out bool deleteExisting)
        {
            TaskDialog decide = new TaskDialog("Pipe Data Received");
            decide.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Replace Objects",
                "Objects in the scene will be deleted and replaced with the new objects");
            decide.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Update Geometry",
                "The objects will not be deleted, but their geometry will be updated.");
            decide.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Append New Geometry",
                "New geometry will be added to the document without changing the existing geometry.");
            TaskDialogResult result = decide.Show();
            deleteExisting = result == TaskDialogResult.CommandLink1;
            return result == TaskDialogResult.CommandLink2;
        }

        private bool GeometryTypeMatch()
        {
            if(_receivedObjects.Count != _previousIds.Count) { return false; }
            if(_receivedObjects.Count != _previousRefs.Count) { return false; }
            for (int i = 0; i < _receivedObjects.Count; i++)
            {
                Element oldElem = _document.GetElement(_previousIds[i]);
                GeometryObject oldGeom = oldElem.GetGeometryObjectFromReference(_previousRefs[i]);
                if(oldGeom.GetType() != _receivedObjects[i].GetType()) { return false; }
            }

            return true;
        }

        private List<ElementId> AddObjectsToDocument(List<GeometryObject> objs, bool deleteExisting = false)
        {
            List<ElementId> elems = new List<ElementId>();
            _previousRefs = new List<Reference>();
            Transaction trans = new Transaction(_document);
            trans.Start("pipe_pull");
            if (deleteExisting)
            {
                List<ElementId> _oldStuff = _previousIds.Where((id) => _document.GetElement(id) != null).ToList();
                _document.Delete(_oldStuff);
            }
            foreach (var geom in _receivedObjects)
            {
                if (typeof(Curve).IsAssignableFrom(geom.GetType()))
                {
                    Reference geomRef;
                    elems.Add(RevitPipeUtil.AddCurveToDocument(ref _document, (Curve)geom, out geomRef));
                    _previousRefs.Add(geomRef);
                }
            }
            trans.Commit();

            return elems;
        }

        private void UpdateGeometry(List<GeometryObject> geomObjs)
        {
            if(geomObjs.Count != _previousIds.Count || geomObjs.Count != _previousRefs.Count)
            {
                throw new InvalidOperationException("Cannot update geometry, counts dont match");
            }

            Transaction trans = new Transaction(_document);
            trans.Start("pipe_pull_update");
            for (int i = 0; i < geomObjs.Count; i++)
            {
                Element elem = _document.GetElement(_previousIds[i]);
                GeometryObject oldGeom = elem.GetGeometryObjectFromReference(_previousRefs[i]);
                if(oldGeom.GetType() != geomObjs[i].GetType()) { continue; }
                if(typeof(ModelCurve).IsAssignableFrom(elem.GetType()) && typeof(Curve).IsAssignableFrom(geomObjs[i].GetType()))
                {
                    ModelCurve curveElem = (ModelCurve)elem;
                    curveElem.SetGeometryCurve((Curve)geomObjs[i], true);
                }
            }
            trans.Commit();
        }
    }
}
