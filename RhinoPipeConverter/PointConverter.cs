using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using rh = Rhino.Geometry;
using pp = PipeDataModel.Types.Geometry;
using PipeDataModel.Types;

namespace RhinoPipeConverter
{
    public class PointConverter:PipeConverter<rh.Point, pp.Vec>
    {
        private Point3dConverter _ptConv;
        public PointConverter(Point3dConverter ptConv)
        {
            _ptConv = ptConv;
            AddConverter(new PipeConverter<rh.Point, pp.Vec>(
                    (rhpt) =>
                    {
                        return _ptConv.ToPipe<rh.Point3d, pp.Vec>(rhpt.Location);
                    },
                    (ppVec) =>
                    {
                        return new rh.Point(_ptConv.FromPipe<rh.Point3d, pp.Vec>(ppVec));
                    }
                ));
        }
    }

    public class Point3dConverter : PipeConverter<rh.Point3d, pp.Vec>
    {
        public Point3dConverter():
            base(
                    (rhpt) =>
                    {
                        return new pp.Vec(rhpt.X, rhpt.Y, rhpt.Z);
                    },
                    (ppVec) =>
                    {
                        pp.Vec v = pp.Vec.Ensure3D(ppVec);
                        List<double> coords = v.Coordinates;
                        return new rh.Point3d(coords[0], coords[1], coords[2]);
                    }
                 )
        { }
    }
}
