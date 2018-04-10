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
            var arcConv = new ArcConverter(ptConv, vecConv);
            AddConverter(arcConv);

            //to convert nurbs curves
            var nurbsConv = new PipeConverter<dg.NurbsCurve, ppc.NurbsCurve>(
                (dc) => {
                    ////just to smooth out anything weird about this curve
                    //dc = dc.ToNurbsCurve();

                    ppc.NurbsCurve cur;
                    List<dg.Point> pts = dc.ControlPoints().ToList();
                    List<double> knots = dc.Knots().ToList();
                    
                    var startParam = dc.StartParameter();
                    var endParam = dc.EndParameter();
                    knots = knots.Select((k) => (k - startParam) / (endParam - startParam)).ToList();
                    //making sure all the weights are not zeros by setting them to 1 if they are
                    double tolerance = 1e-4;
                    List<double> weights = dc.Weights().ToList();
                    if(weights.Any((w) => w <= tolerance)) { weights = weights.Select((w) => 1.0).ToList(); }
                    cur = new ppc.NurbsCurve(pts.Select((pt) => ptConv.ToPipe<dg.Point, ppg.Vec>(pt)).ToList(),
                            dc.Degree, weights, knots, dc.IsClosed);

                    return cur;
                },
                (pnc) => {
                    dg.NurbsCurve cur;
                    try
                    {
                        cur = dg.NurbsCurve.ByControlPointsWeightsKnots(
                            pnc.ControlPoints.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)),
                            pnc.Weights.ToArray(), pnc.Knots.ToArray(), pnc.Degree);
                        if(pnc.IsClosed != cur.IsClosed)
                        {
                            cur = dg.NurbsCurve.ByControlPoints(
                                pnc.ControlPoints.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)), pnc.Degree,
                                pnc.IsClosed);
                        }
                    }
                    catch (Exception e)
                    {
                        cur = dg.NurbsCurve.ByControlPoints(
                            pnc.ControlPoints.Select((pt) => ptConv.FromPipe<dg.Point, ppg.Vec>(pt)), pnc.Degree,
                            pnc.IsClosed);
                    }
                    return cur;
                }
            );
            AddConverter(nurbsConv);

            //generic curves - one way conversion, hence one of the conversion delegates is null
            AddConverter(new PipeConverter<dg.Curve, ppc.Curve>(
                (dc) => {
                    return nurbsConv.ToPipe<dg.NurbsCurve, ppc.NurbsCurve>(dc.ToNurbsCurve());
                },
                null
            ));

            //conversion for polycurves
            var polyCurveConv = new PipeConverter<dg.PolyCurve, ppc.PolyCurve>(
                (dpc) => {
                    dg.Curve[] curs = dpc.Curves();
                    return new ppc.PolyCurve(curs.Select((c) => ToPipe<dg.Curve, ppc.Curve>(c)).ToList());
                },
                (ppcrv) => {
                    return dg.PolyCurve.ByJoinedCurves(ppcrv.Segments.Select((c) => FromPipe<dg.Curve, ppc.Curve>(c)));
                }
            );
            AddConverter(polyCurveConv);
            //one way conversion for incoming polylines
            var plineConv = new PipeConverter<dg.PolyCurve, ppc.Polyline>(
                null,
                (ppl) => {
                    return polyCurveConv.FromPipe<dg.PolyCurve, ppc.PolyCurve>(ppl.AsPolyCurve());
                }
            );
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
}
