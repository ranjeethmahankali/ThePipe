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
        internal static Plane GetPlaneForCurve(Curve curve)
        {
            if (typeof(Line).IsAssignableFrom(curve.GetType()))
            {
                return GetPlaneForLine((Line)curve);
            }
            else
            {
                XYZ startPt = curve.GetEndPoint(0);
                XYZ intPt = curve.Evaluate(0.5, true);
                XYZ endPt = curve.GetEndPoint(1);

                XYZ v1 = endPt.Subtract(startPt);
                XYZ v2 = intPt.Subtract(startPt);
                XYZ normal = v1.CrossProduct(v2);
                return Plane.CreateByNormalAndOrigin(normal, startPt);
            }
        }

        internal static Plane GetPlaneForLine(Line line)
        {
            double eps = 1e-7;
            XYZ normal;
            if(Math.Abs(line.Direction.DotProduct(XYZ.BasisZ)) < eps)
            {
                normal = XYZ.BasisZ;
            }
            else
            {
                normal = line.Direction.CrossProduct(XYZ.BasisZ);
            }
            return Plane.CreateByNormalAndOrigin(normal, line.Origin);
        }

        internal static ElementId AddCurveToDocument(ref Document doc, Curve curve, out Reference geomRef)
        {
            SketchPlane plane = SketchPlane.Create(doc, GetPlaneForCurve(curve));
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
