using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry;
using PipeDataModel.Types;
using pipeGeom = PipeDataModel.Types.Geometry;

using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace PipeForGrasshopper
{
    public class ConvertToPipe
    {
        public static PipeData ConvertObject(IGH_Goo obj)
        {
            if (typeof(GH_Number).IsAssignableFrom(obj.GetType()))
            {
                return new PipeData(((GH_Number)obj).Value);
            }
            else if (typeof(GH_String).IsAssignableFrom(obj.GetType()))
            {
                return new PipeData(((GH_String)obj).Value);
            }
            else if (typeof(GH_Point).IsAssignableFrom(obj.GetType()))
            {
                return new PipeData(ConvertPoint((GH_Point)obj));
            }
            else if (typeof(GH_Line).IsAssignableFrom(obj.GetType()))
            {
                return new PipeData(ConvertLine((GH_Line)obj));
            }
            else { return new PipeData(null); }
        }

        //converting points
        public static Vec ConvertPoint(GH_Point point)
        {
            return new Vec(point.Value.X, point.Value.Y, point.Value.Z);
        }
        public static Vec ConvertPoint(Point2d pt)
        {
            return new Vec(pt.X, pt.Y);
        }
        public static Vec ConvertPoint(Point2f pt)
        {
            return new Vec(pt.X, pt.Y);
        }
        public static Vec ConvertPoint(Point3d pt)
        {
            return new Vec(pt.X, pt.Y, pt.Z);
        }
        public static Vec ConvertPoint(Point3f pt)
        {
            return new Vec(pt.X, pt.Y, pt.Z);
        }

        //converting lines
        public static pipeGeom.Curve.Line ConvertLine(GH_Line line)
        {
            return new pipeGeom.Curve.Line(ConvertPoint(line.Value.From), ConvertPoint(line.Value.To));
        }
        public static pipeGeom.Curve.Line ConvertLine(Line line)
        {
            return new pipeGeom.Curve.Line(ConvertPoint(line.From), ConvertPoint(line.To));
        }
    }
}
