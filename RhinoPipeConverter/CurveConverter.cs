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
        public CurveConverter(Point3dConverter ptConv, ArcConverter arcConv, LineConverter lineConv)
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
            //to convert polyline curves
            AddConverter(new PipeConverter<rh.PolylineCurve, pp.Polyline>(
                    (rhpl) =>
                    {
                        List<ppg.Vec> ptList = new List<ppg.Vec>();
                        int ptCount = rhpl.PointCount;
                        for (int i = 0; i < ptCount; i++)
                        {
                            ptList.Add(ptConv.ToPipe<rh.Point3d, ppg.Vec>(rhpl.Point(i)));
                        }
                        return new pp.Polyline(ptList);
                    },
                    (ppl) =>
                    {
                        List<rh.Point3d> ptList = new List<rh.Point3d>();
                        foreach (var pt in ppl.Points)
                        {
                            ptList.Add(ptConv.FromPipe<rh.Point3d, ppg.Vec>(pt));
                        }
                        return new rh.PolylineCurve(ptList);
                    }
                ));
            //to convert nurbs curves
            AddConverter(new PipeConverter<rh.NurbsCurve, pp.NurbsCurve>(
                    (rhc) => {
                        pp.NurbsCurve curve;
                        if (rhc.IsRational)
                        {
                            curve = new pp.NurbsCurve(rhc.Points.Select((pt) => ptConv.ToPipe<rh.Point3d,
                            ppg.Vec>(pt.Location)).ToList(), rhc.Degree);
                        }
                        else
                        {
                            curve = new pp.NurbsCurve(rhc.Points.Select((pt) => ptConv.ToPipe<rh.Point3d,
                            ppg.Vec>(pt.Location)).ToList(), rhc.Degree, 
                            rhc.Points.Select((pt) => pt.Weight).ToList(), rhc.Knots.ToList());
                        }
                        return curve;
                    },
                    (ppc) => {
                        rh.NurbsCurve curve = (rh.NurbsCurve)rh.Curve.CreateControlPointCurve(
                            ppc.ControlPoints.Select((pt) => ptConv.FromPipe<rh.Point3d, ppg.Vec>(pt)),
                            ppc.Degree);
                        if (!ppc.IsRational)
                        {
                            for(int i = 0; i < curve.Points.Count; i++)
                            {
                                var pt = curve.Points.ElementAt(i);
                                var newPt = new rh.Point4d(pt.Location.X, pt.Location.Y, pt.Location.Z, pt.Weight);
                                curve.Points.SetPoint(i, newPt);
                            }
                        }
                        return curve;
                    }
                ));

            //to convert polycurves
            AddConverter(new PipeConverter<rh.PolyCurve, pp.PolyCurve>(
                    (rhc) => {
                        List<pp.Curve> curves = new List<pp.Curve>();
                        for(int i = 0; i < rhc.SegmentCount; i++)
                        {
                            curves.Add(ToPipe<rh.Curve, pp.Curve>(rhc.SegmentCurve(i)));
                        }
                        return new pp.PolyCurve(curves);
                    },
                    (ppc) => {
                        var curve = new rh.PolyCurve();
                        foreach(var segment in ppc.Segments)
                        {
                            curve.Append(FromPipe<rh.Curve, pp.Curve>(segment));
                        }
                        return curve;
                    }
                ));
        }
    }

    public class ArcConverter : PipeConverter<rh.Arc, pp.Arc>
    {
        public ArcConverter(PlaneConverter planeConv, Point3dConverter ptConv) :
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
                            ppArc.EndAngle - ppArc.StartAngle);
                    }
                )
        { }
    }

    public class LineConverter : PipeConverter<rh.Line, pp.Line>
    {
        public LineConverter(Point3dConverter ptConv) :
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

    public class PolylineConverter:PipeConverter<rh.Polyline, pp.Polyline>
    {
        public PolylineConverter(Point3dConverter ptConv):
            base(
                    (rhpl) =>
                    {
                        List<ppg.Vec> ptList = new List<ppg.Vec>();
                        int ptCount = rhpl.Count;
                        for(int i = 0; i < ptCount; i++)
                        {
                            ptList.Add(ptConv.ToPipe<rh.Point3d, ppg.Vec>(rhpl[i]));
                        }
                        return new pp.Polyline(ptList);
                    },
                    (ppl) =>
                    {
                        List<rh.Point3d> ptList = new List<rh.Point3d>();
                        foreach(var pt in ppl.Points)
                        {
                            ptList.Add(ptConv.FromPipe<rh.Point3d, ppg.Vec>(pt));
                        }
                        return new rh.Polyline(ptList);
                    }
                )
        { }
    }
}
