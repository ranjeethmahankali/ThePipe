using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using pp = PipeDataModel.Types.Geometry;
using rh = Rhino.Geometry;

namespace RhinoV6PipeConverter
{
    public class PlaneConverter: PipeConverter<rh.Plane, pp.Plane>
    {
        public PlaneConverter(Vector3DConverter vecConv, Point3dConverter ptConv):
            base(
                    (rhp) =>
                    {
                        pp.Vec origin = ptConv.ToPipe<rh.Point3d, pp.Vec>(rhp.Origin);
                        pp.Vec x = vecConv.ToPipe<rh.Vector3d, pp.Vec>(rhp.XAxis);
                        pp.Vec y = vecConv.ToPipe<rh.Vector3d, pp.Vec>(rhp.YAxis);
                        //we won't convert the z-axis because we can simply cross the x and y
                        return new pp.Plane(origin, x, y);
                    },
                    (ppp) =>
                    {
                        rh.Point3d origin = ptConv.FromPipe<rh.Point3d, pp.Vec>(ppp.Origin);
                        rh.Vector3d x = vecConv.FromPipe<rh.Vector3d, pp.Vec>(ppp.X);
                        rh.Vector3d y = vecConv.FromPipe<rh.Vector3d, pp.Vec>(ppp.Y);
                        return new rh.Plane(origin, x, y);
                    }
                ) { }
    }
}
