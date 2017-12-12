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
    public class RevitPipeConverter: PipeConverter<object, IPipeMemberType>
    {
        public RevitPipeConverter()
        {
            var ptConv = new PointConverter();
            AddConverter(ptConv);
            var planeConv = AddConverter(new PipeConverter<rg.Plane, ppg.Plane>(
                (rpl) => {
                    return new ppg.Plane(ptConv.ToPipe<rg.XYZ, ppg.Vec>(rpl.Origin), ptConv.ToPipe<rg.XYZ, ppg.Vec>(rpl.XVec),
                        ptConv.ToPipe<rg.XYZ, ppg.Vec>(rpl.YVec), ptConv.ToPipe<rg.XYZ, ppg.Vec>(rpl.Normal));
                },
                (ppl) => {
                    return rg.Plane.CreateByOriginAndBasis(ptConv.FromPipe<rg.XYZ, ppg.Vec>(ppl.Origin),
                        ptConv.FromPipe<rg.XYZ, ppg.Vec>(ppl.X), ptConv.FromPipe<rg.XYZ, ppg.Vec>(ppl.Y));
                }
            ));
            var geomConv = AddConverter(new GeometryConverter(ptConv, planeConv));
        }
    }

    public class GeometryConverter: PipeConverter<rg.GeometryObject, IPipeMemberType>
    {
        public GeometryConverter(PointConverter ptConv, PipeConverter<rg.Plane, ppg.Plane> planeConv)
        {
            //converting various types of curves
            var curveConv = AddConverter(new CurveConverter(ptConv, planeConv));
            //converting polylines: for some reasons polyline class doesn't inherit from curves in revit
            var polylineConv = AddConverter(new PipeConverter<rg.PolyLine, ppc.Polyline>(
                (rpl) => {
                    List<rg.XYZ> pts = rpl.GetCoordinates().ToList();
                    return new ppc.Polyline(pts.Select((pt) => ptConv.ToPipe<rg.XYZ, ppg.Vec>(pt)).ToList());
                },
                (ppl) => {
                    return rg.PolyLine.Create(ppl.Points.Select((pt) => ptConv.FromPipe<rg.XYZ, ppg.Vec>(pt)).ToList());
                }
            ));
        }
    }

    public class PointConverter: PipeConverter<rg.XYZ, ppg.Vec>
    {
        public PointConverter():
            base(
                    (rpt) => {
                        return new ppg.Vec(rpt.X, rpt.Y, rpt.Z);
                    },
                    (ppt) => {
                        var pt = ppg.Vec.Ensure3D(ppt);
                        return new rg.XYZ(pt.Coordinates[0], pt.Coordinates[1], pt.Coordinates[2]);
                    }
                )
        { }
    }
}
