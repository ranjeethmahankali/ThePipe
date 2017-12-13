using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace PipeForRevit.Utils
{
    public class SketchPlaneUtil
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

                if(startPt.DistanceTo(endPt) == 0) { endPt = curve.Evaluate(0.1, true); }

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
            if (Math.Abs(line.Direction.DotProduct(XYZ.BasisZ)) < eps)
            {
                normal = XYZ.BasisZ;
            }
            else
            {
                normal = line.Direction.CrossProduct(XYZ.BasisZ);
            }
            return Plane.CreateByNormalAndOrigin(normal, line.Origin);
        }
    }
}
