using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RhinoV6PipeConverter;
using PipeDataModel.Types;
using Rhino.Geometry;

namespace PipeForRhinoV6
{
    public class PipeConverter
    {
        private static GeometryConverter _converter = new GeometryConverter();

        public static GeometryBase FromPipe(IPipeMemberType pp)
        {
            return _converter.FromPipe<GeometryBase, IPipeMemberType>(pp);
        }
        public static IPipeMemberType ToPipe(GeometryBase obj)
        {
            return _converter.ToPipe<GeometryBase, IPipeMemberType>(obj);
        }
    }
}
