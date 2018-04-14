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
    public class SurfaceConverter: PipeConverter<rg.Solid, pps.Surface>
    {
        public SurfaceConverter(PointConverter ptConv, CurveConverter curveConv) :base(
            (rs) => {
                throw new NotImplementedException();
            },
            (ps) => {
                var polySurf = ps.AsPolySurface();
                rg.BRepBuilder brepBuider = new rg.BRepBuilder(polySurf.IsSolid ? rg.BRepType.Solid : rg.BRepType.OpenShell);
                //build the brep
                foreach(var surf in polySurf.Surfaces)
                {
                    rg.BoundingBoxUV uvBound = new rg.BoundingBoxUV();
                    if (!typeof(pps.NurbsSurface).IsAssignableFrom(surf.GetType())) { continue; }
                    var ppNurbs = (pps.NurbsSurface)surf;
                    var faceId = brepBuider.AddFace(rg.BRepBuilderSurfaceGeometry.CreateNURBSSurface(ppNurbs.UDegree, ppNurbs.VDegree, 
                        ppNurbs.UKnots, ppNurbs.VKnots, ppNurbs.GetControlPointsAsList().Select((pt) => 
                        ptConv.FromPipe<rg.XYZ, ppg.Vec>(pt)).ToList(), false, uvBound), false);

                    //var edges = ppNurbs.Edges();
                    //foreach(var edge in edges)
                    //{

                    //}
                }

                return brepBuider.GetResult();
            }
        ){ }
    }
}
