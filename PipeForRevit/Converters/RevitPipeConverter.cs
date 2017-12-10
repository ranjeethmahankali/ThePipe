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
    public class RevitPipeConverter: PipeConverter<rg.GeometryObject, IPipeMemberType>
    {
        private static PointConverter _ptConv = new PointConverter();

        public RevitPipeConverter()
        {
            var curveConv = AddConverter(new CurveConverter(_ptConv));
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
