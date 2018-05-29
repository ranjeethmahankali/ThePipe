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
    public class SurfaceConverter: PipeConverter<rg.Solid, IPipeMemberType>
    {
        public SurfaceConverter(PointConverter ptConv, CurveConverter curveConv, MeshConverter meshConv)
        {
            var polySurfToSolidConv = AddConverter(new PipeConverter<rg.Solid, pps.PolySurface>(
                null,
                (ps) => {
                    rg.BRepBuilder brepBuider = new rg.BRepBuilder(ps.IsSolid ? rg.BRepType.Solid : rg.BRepType.OpenShell);
                    //build the brep
                    foreach (var surf in ps.Surfaces)
                    {
                        rg.BoundingBoxUV uvBound = new rg.BoundingBoxUV();
                        if (!typeof(pps.NurbsSurface).IsAssignableFrom(surf.GetType())) { continue; }
                        var ppNurbs = (pps.NurbsSurface)surf;
                        var faceId = brepBuider.AddFace(rg.BRepBuilderSurfaceGeometry.CreateNURBSSurface(ppNurbs.UDegree, ppNurbs.VDegree,
                            ppNurbs.UKnots, ppNurbs.VKnots, ppNurbs.GetControlPointsAsList().Select((pt) =>
                            ptConv.FromPipe<rg.XYZ, ppg.Vec>(pt)).ToList(), false, uvBound), false);

                        var loopId = brepBuider.AddLoop(faceId);
                        var edges = ppNurbs.Edges();
                        foreach (var edge in edges)
                        {
                            var edgeId = brepBuider.AddEdge(rg.BRepBuilderEdgeGeometry.Create(curveConv.FromPipe<rg.Curve, ppc.Curve>(edge)));
                            brepBuider.AddCoEdge(loopId, edgeId, false);
                        }
                        brepBuider.FinishLoop(loopId);
                        brepBuider.FinishFace(faceId);
                    }
                    brepBuider.Finish();
                    return brepBuider.GetResult();
                }
            ));

            var solidToMeshConv = AddConverter(new PipeConverter<rg.Solid, ppg.GeometryGroup>(
                (rs) => {
                    ppg.GeometryGroup gg = new ppg.GeometryGroup();
                    foreach(rg.Face face in rs.Faces)
                    {
                        gg.AddMember(meshConv.ToPipe<rg.Mesh, ppg.Mesh>(face.Triangulate()));
                    }
                    return gg;
                },
                null
            ));
        }
    }

    public class NurbsSurfaceConverter:PipeConverter<rg.NurbsSurfaceData, pps.NurbsSurface>
    {
        public NurbsSurfaceConverter(): base(
            (rns) => {

                //incomplete
                throw new NotImplementedException();
            },
            (pns) => {
                //incomplete
                throw new NotImplementedException();
            }
        )
        { }
    }
}
