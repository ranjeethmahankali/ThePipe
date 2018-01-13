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

                Random rand = new Random();
                while (normal.IsZeroLength())
                {
                    intPt = curve.Evaluate(rand.NextDouble(), true);
                    v1 = endPt.Subtract(startPt);
                    v2 = intPt.Subtract(startPt);
                    normal = v1.CrossProduct(v2);
                }
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

        internal static Plane GetPlaneForPolyLine(PolyLine pline)
        {
            double eps = 1e-7;
            var ptList = pline.GetCoordinates().ToList();
            var pt0 = ptList[0];
            XYZ normal = null;
            for(int i = 1; i < ptList.Count - 3; i++)
            {
                XYZ v1 = ptList[i + 1].Subtract(ptList[i]);
                XYZ v2 = ptList[i + 2].Subtract(ptList[i]);
                XYZ v3 = ptList[i + 3].Subtract(ptList[i]);

                if(v1.DotProduct(v2.CrossProduct(v3)) > eps)
                {
                    throw new ArgumentException("The polyline is not planar, so it cannot be added to the Revit file");
                }

                normal = v1.CrossProduct(v2);
                if (normal.IsZeroLength()) { normal = v2.CrossProduct(v3); }
                if (normal.IsZeroLength()) { normal = v1.CrossProduct(v3); }
                if (normal.IsZeroLength()) { normal = null; }
            }

            if(normal == null || normal.IsZeroLength())
            {
                return GetPlaneForLine(Line.CreateBound(ptList[0], ptList[2]));
            }

            return Plane.CreateByNormalAndOrigin(normal, ptList[0]);
        }
    }
}
