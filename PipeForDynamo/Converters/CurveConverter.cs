using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using dg = Autodesk.DesignScript.Geometry;

namespace PipeForDynamo.Converters
{
    //public class CurveConverter : PipeConverter<dg.Curve, ppc.Curve>
    //{
    //    public CurveConverter(PointConverter ptConv)
    //    {
    //        var lineConv = new LineConverter(ptConv);
    //        AddConverter(lineConv);
    //    }
    //}

    //public class LineConverter : PipeConverter<dg.Line, ppc.Line>
    //{
    //    public LineConverter(PointConverter ptConv) :
    //        base(
    //                (ln) =>
    //                {
    //                    return new ppc.Line(ptConv.ToPipe<dg.Point, ppg.Vec>(ln.StartPoint),
    //                        ptConv.ToPipe<dg.Point, ppg.Vec>(ln.EndPoint));
    //                },
    //                (pln) =>
    //                {
    //                    return dg.Line.ByStartPointEndPoint(ptConv.FromPipe<dg.Point, ppg.Vec>(pln.StartPoint),
    //                        ptConv.FromPipe<dg.Point, ppg.Vec>(pln.EndPoint));
    //                }
    //            )
    //    { }
    //}
}
