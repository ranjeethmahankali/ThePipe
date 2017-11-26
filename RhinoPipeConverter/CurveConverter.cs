using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using pp = PipeDataModel.Types.Geometry.Curve;
using ppg = PipeDataModel.Types.Geometry;
using rh = Rhino.Geometry;

namespace RhinoPipeConverter
{
    public class CurveConverter:PipeConverter<rh.Curve, pp.Curve>
    {
        public CurveConverter(ArcConverter arcConv, LineConverter lineConv)
        {
            //to convert ArcCurves
            AddConverter(new PipeConverter<rh.ArcCurve,pp.Arc>(
                    (rhArc) => { return arcConv.ToPipe<rh.Arc, pp.Arc>(rhArc.Arc); },
                    (ppArc) => { return new rh.ArcCurve(arcConv.FromPipe<rh.Arc, pp.Arc>(ppArc)); }
                ));
            //to convert LineCurves
            AddConverter(new PipeConverter<rh.LineCurve, pp.Line>(
                    (rhLine) => { return lineConv.ToPipe<rh.Line, pp.Line>(rhLine.Line); },
                    (ppLine) => { return new rh.LineCurve(lineConv.FromPipe<rh.Line, pp.Line>(ppLine)); }
                ));
        }

        public class ArcConverter: PipeConverter<rh.Arc, pp.Arc>
        {
            public ArcConverter(PlaneConverter planeConv, Point3dConverter ptConv):
                base(
                        (rhArc) =>
                        {
                            ppg.Plane pl = planeConv.ToPipe<rh.Plane, ppg.Plane>(rhArc.Plane);
                            ppg.Plane pl2 = new ppg.Plane(ptConv.ToPipe<rh.Point3d, ppg.Vec>(rhArc.Center), pl.X, pl.Y);
                            return new pp.Arc(pl2, rhArc.Radius, rhArc.StartAngle, rhArc.EndAngle);
                        },
                        (ppArc) =>
                        {
                            return new rh.Arc(planeConv.FromPipe<rh.Plane, ppg.Plane>(ppArc.Plane), ppArc.Radius, 
                                ppArc.EndAngle-ppArc.StartAngle);
                        }
                    )
            { }
        }

        public class LineConverter: PipeConverter<rh.Line, pp.Line>
        {
            public LineConverter(Point3dConverter ptConv):
                base(
                        (rhLine) => 
                        {
                            return new pp.Line(ptConv.ToPipe<rh.Point3d, ppg.Vec>(rhLine.From),
                                ptConv.ToPipe<rh.Point3d, ppg.Vec>(rhLine.To));
                        },
                        (ppLine) => 
                        {
                            return new rh.Line(ptConv.FromPipe<rh.Point3d, ppg.Vec>(ppLine.StartPoint),
                                ptConv.FromPipe<rh.Point3d, ppg.Vec>(ppLine.EndPoint));
                        }
                    )
            { }
        }
    }
}
