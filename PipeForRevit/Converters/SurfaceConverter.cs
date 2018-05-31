using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using pps = PipeDataModel.Types.Geometry.Surface;

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
    //public class SurfaceConverter: PipeConverter<rg.Face, ppg.Mesh>
    //{
    //    public SurfaceConverter(PointConverter ptConv, CurveConverter curveConv, MeshConverter meshConv): base(
    //        (rf) => {
    //            return meshConv.ToPipe<rg.Mesh, ppg.Mesh>(rf.Triangulate());
    //        },
    //        null
    //    )
    //    { }
    //}

    public class SolidConverter: PipeConverter<rg.Solid, ppg.Mesh>
    {
        public SolidConverter(MeshConverter meshConv): base(
            (rs) => {
                //return meshConv.ToPipe<rg.Mesh, ppg.Mesh>(rf.Triangulate());
                ppg.Mesh mesh = null;
                foreach(rg.Face face in rs.Faces)
                {
                    var faceMesh = meshConv.ToPipe<rg.Mesh, ppg.Mesh>(face.Triangulate());
                    if (mesh == null)
                    {
                        mesh = faceMesh;
                    }
                    else
                    {
                        mesh.MergeMesh(faceMesh);
                    }
                }

                return mesh;
            },
            null
        ) { }
    }
}
