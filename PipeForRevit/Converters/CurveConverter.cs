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
        public CurveConverter(PointConverter ptConv, PipeConverter<rg.Plane, ppg.Plane> planeConv)
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
            //converting arcs
            var arcConv = AddConverter(new PipeConverter<rg.Arc, ppc.Arc>(
                (rarc) => {
                    ppg.Vec startPt = ptConv.ToPipe<rg.XYZ, ppg.Vec>(rarc.GetEndPoint(0));
                    ppg.Vec endPt = ptConv.ToPipe<rg.XYZ, ppg.Vec>(rarc.GetEndPoint(1));
                    ppg.Vec midPt = ptConv.ToPipe<rg.XYZ, ppg.Vec>(rarc.Evaluate(0.5, true));
                    return new ppc.Arc(startPt, endPt, midPt);
                },
                (parc) => {
                    return rg.Arc.Create(planeConv.FromPipe<rg.Plane, ppg.Plane>(parc.Plane), parc.Radius, 
                        parc.StartAngle, parc.EndAngle);
                }
            ));
        }
    }
}
