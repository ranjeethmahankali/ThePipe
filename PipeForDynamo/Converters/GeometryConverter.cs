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
    public class GeometryConverter: PipeConverter<dg.Geometry, IPipeMemberType>
    {
        public GeometryConverter():
            base()
        {
            var ptConv = new PointConverter();
            AddConverter(ptConv);
            //var curConv = new CurveConverter(ptConv);
            //AddConverter(curConv);
        }
    }

    public class PointConverter: PipeConverter<dg.Point, ppg.Vec>
    {
        public PointConverter():
            base(
                    (pt) => { return new ppg.Vec(pt.X, pt.Y, pt.Z); },
                    (ppt) => {
                        var pt = ppg.Vec.Ensure3D(ppt);
                        return dg.Point.ByCoordinates(pt.Coordinates[0], pt.Coordinates[1], pt.Coordinates[2]);
                    }
                )
        { }
    }
}
