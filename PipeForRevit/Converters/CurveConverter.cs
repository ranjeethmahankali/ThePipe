using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;

using rg = Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.DB.ExternalService;

namespace PipeForRevit.Converters
{
    public class CurveConverter: PipeConverter<rg.Curve, ppc.Curve>
    {
        public CurveConverter(PointConverter ptConv)
        {
            //converting lines
            var lineConv = AddConverter(new PipeConverter<rg.Line, ppc.Line>(
                (rl) => {
                    return new ppc.Line(ptConv.ToPipe<rg.XYZ, ppg.Vec>(rl.GetEndPoint(0)),
                        ptConv.ToPipe<rg.XYZ, ppg.Vec>(rl.GetEndPoint(1)));
                },
                (ppl) => {
                    return rg.Line.CreateBound(ptConv.FromPipe<rg.XYZ, ppg.Vec>(ppl.StartPoint),
                        ptConv.FromPipe<rg.XYZ, ppg.Vec>(ppl.EndPoint));
                }
            ));
        }
    }
}
