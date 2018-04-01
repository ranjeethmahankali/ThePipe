using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using dg = Autodesk.DesignScript.Geometry;
using PipeDataModel.Utils;

namespace PipeForDynamo.Converters
{
    internal class CurveConverter : PipeConverter<dg.Curve, ppc.Curve>
    {
        internal CurveConverter(PointConverter ptConv, VectorConverter vecConv)
        {
            var lineConv = new LineConverter(ptConv);
            AddConverter(lineConv);
            var plineConv = new PolylineConverter(ptConv);
            AddConverter(plineConv);
            var pcurveConv = new PolyCurveConverter(this);
            AddConverter(pcurveConv);
            var arcConv = new ArcConverter(ptConv, vecConv);
            AddConverter(arcConv);

            //to convert nurbs curves
            AddConverter(new PipeConverter<dg.NurbsCurve, ppc.NurbsCurve>(
                    (dc) => {
                        ppc.NurbsCurve cur;
                        List<dg.Point> pts = dc.ControlPoints().ToList();
                        List<double> knots = dc.Knots().ToList();

                        var startParam = dc.StartParameter();
                        var endParam = dc.EndParameter();
                        knots = knots.Select((k) => (k-startParam)/(endParam - startParam)).ToList();

                        cur = new ppc.NurbsCurve(pts.Select((pt) => ptConv.ToPipe<dg.Point, ppg.Vec>(pt)).ToList(),
                                dc.Degree, dc.Weights().ToList(), knots, dc.IsClosed);

                        return cur;
                    },
                    (pnc) => {
                        dg.NurbsCurve cur;
                        if (pnc.IsRational)
                        {
                            cur = dg.NurbsCurve.ByControlPoints(
                                pnc.ControlPoints.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)),pnc.Degree,
                                pnc.IsClosed);
                        }
                        else
                        {
                            cur = dg.NurbsCurve.ByControlPointsWeightsKnots(
                                pnc.ControlPoints.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)),
                                pnc.Weights.ToArray(), pnc.Knots.ToArray(), pnc.Degree);
                        }
                        return cur;
                    }
                ));
        }
    }

    internal class LineConverter : PipeConverter<dg.Line, ppc.Line>
    {
        internal LineConverter(PointConverter ptConv) :
            base(
                    (ln) =>
                    {
                        return new ppc.Line(ptConv.ToPipe<dg.Point, ppg.Vec>(ln.StartPoint),
                            ptConv.ToPipe<dg.Point, ppg.Vec>(ln.EndPoint));
                    },
                    (pln) =>
                    {
                        return dg.Line.ByStartPointEndPoint(ptConv.FromPipe<dg.Point, ppg.Vec>(pln.StartPoint),
                            ptConv.FromPipe<dg.Point, ppg.Vec>(pln.EndPoint));
                    }
                )
        { }
    }

    internal class PolylineConverter: PipeConverter<dg.PolyCurve, ppc.Polyline>
    {
        internal PolylineConverter(PointConverter ptConv):
            base(
                    (dpc) => {
                        dg.Curve[] curs = dpc.Curves();
                        List<ppg.Vec> pts = new List<ppg.Vec>();
                        if (curs.Length == 0) { return new ppc.Polyline(pts); }

                        pts.Add(ptConv.ToPipe<dg.Point, ppg.Vec>(curs[0].StartPoint));
                        for(int i = 0; i < curs.Length; i++)
                        {
                            pts.Add(ptConv.ToPipe<dg.Point, ppg.Vec>(curs[i].EndPoint));
                        }

                        return new ppc.Polyline(pts);
                    },
                    (ppl) => {
                        return dg.PolyCurve.ByPoints(ppl.Points.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)));
                    }
                )
        { }
    }

    internal class ArcConverter: PipeConverter<dg.Arc, ppc.Arc>
    {
        internal ArcConverter(PointConverter ptConv, VectorConverter vecConv):
            base(
                    (dgarc) => {
                        ppg.Plane pl = new ppg.Plane(ptConv.ToPipe<dg.Point, ppg.Vec>(dgarc.CenterPoint));
                        return new ppc.Arc(pl, dgarc.Radius, dgarc.StartAngle, dgarc.StartAngle + dgarc.SweepAngle);
                    },
                    (pparc) => {
                        pparc.TransformToPlane(new ppg.Plane(pparc.Plane.Origin, pparc.Plane.Z));
                        return dg.Arc.ByCenterPointRadiusAngle(ptConv.FromPipe<dg.Point, ppg.Vec>(pparc.Plane.Origin), pparc.Radius, 
                            PipeDataUtil.RadiansToDegrees(pparc.StartAngle), PipeDataUtil.RadiansToDegrees(pparc.EndAngle), 
                            vecConv.FromPipe<dg.Vector, ppg.Vec>(pparc.Plane.Z));
                    }
                )
        { }
    }

    internal class PolyCurveConverter: PipeConverter<dg.PolyCurve, ppc.PolyCurve>
    {
        internal PolyCurveConverter(CurveConverter curConv): 
            base(
                    (dpc) => {
                        dg.Curve[] curs = dpc.Curves();
                        return new ppc.PolyCurve(curs.Select((c) => curConv.ToPipe<dg.Curve, ppc.Curve>(c)).ToList());
                    },
                    (ppc) => {
                        return dg.PolyCurve.ByJoinedCurves(ppc.Segments.Select((s) => curConv.FromPipe<dg.Curve, ppc.Curve>(s)));
                    }
                )
        { }
    }
}
