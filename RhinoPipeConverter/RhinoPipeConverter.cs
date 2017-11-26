using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using rh = Rhino.Geometry;


namespace RhinoPipeConverter
{
    public class GeometryConverter:PipeConverter<rh.GeometryBase, IPipeMemberType>
    {
        private Point3dConverter _pt3dConv;

        public GeometryConverter()
        {
            _pt3dConv = new Point3dConverter();
            var ptConv = AddConverter(new PointConverter(_pt3dConv));
            AddConverter(new CurveConverter());
        }
    }
}
