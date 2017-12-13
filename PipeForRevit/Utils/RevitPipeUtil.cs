using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using PipeDataModel.Types;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace PipeForRevit.Utils
{
    public class RevitPipeUtil
    {
        internal static ElementId AddCurveToDocument(ref Document doc, Curve curve, out Reference geomRef)
        {
            SketchPlane plane = SketchPlane.Create(doc, SketchPlaneUtil.GetPlaneForCurve(curve));
            ModelCurve addedCurve = doc.Create.NewModelCurve(curve, plane);
            geomRef = addedCurve.GeometryCurve.Reference;
            return addedCurve.Id;
        }

        internal static void ShowMessage(string title, string instruction = "", string message = "")
        {
            TaskDialog box = new TaskDialog(title);
            box.MainInstruction = instruction;
            box.MainContent = message;
            box.Show();
        }

        internal static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            //resolving the type assemblies
            return typeof(IPipeMemberType).Assembly;
        }
    }
}
