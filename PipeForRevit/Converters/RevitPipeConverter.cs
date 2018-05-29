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

            var geomConv = new GeometryConverter(ptConv, planeConv);
            AddConverter(geomConv);

            var polylineListConv = AddConverter(new PipeConverter<rg.Line[], ppc.Polyline>(
                (rlg) =>
                {
                    List<rg.XYZ> pts = rlg.Select((ln) => ln.GetEndPoint(0)).ToList();
                    pts.Add(rlg.LastOrDefault().GetEndPoint(1));
                    return new ppc.Polyline(pts.Select((pt) => ptConv.ToPipe<rg.XYZ, ppg.Vec>(pt)).ToList());
                },
                (ppl) =>
                {
                    List<ppc.Line> lines = ppl.ExplodedLines();
                    return lines.Select((ln) => (rg.Line)geomConv.FromPipe<rg.GeometryObject, IPipeMemberType>(ln)).ToArray();
                }
            ));

            //extrusions
            var extrConv = AddConverter(new PipeConverter<rg.Extrusion, pps.Extrusion>(
                    (rext) => {
                        rg.XYZ norm = rext.Sketch.SketchPlane.GetPlane().Normal.Normalize();
                        var startNorm = norm.Multiply(rext.StartOffset);
                        var endNorm = norm.Normalize().Multiply(rext.EndOffset);
                        var depthVec = norm.Normalize().Multiply(rext.EndOffset - rext.StartOffset);

                        List<ppc.Curve> curs = new List<ppc.Curve>();
                        foreach (rg.Curve cur in rext.Sketch.Profile)
                        {
                            curs.Add(geomConv.CurveConverter.ToPipe<rg.Curve, ppc.Curve>(cur.CreateTransformed(
                                rg.Transform.CreateTranslation(startNorm))));
                        }

                        return new pps.Extrusion(new ppc.PolyCurve(curs), ptConv.ToPipe<rg.XYZ, ppg.Vec>(depthVec),
                            rext.EndOffset - rext.StartOffset);
                    },
                    (pe) => {
                        double tolerance = 1e-3;
                        rg.CurveArrArray profile = new rg.CurveArrArray();
                        rg.CurveArray curs = new rg.CurveArray();
                        List<ppc.Curve> pCurs = pe.ProfileCurve.FlattenedCurveList();
                        rg.Plane plane = null;
                        pCurs.ForEach((cur) => {
                            rg.Curve revitCur = geomConv.CurveConverter.FromPipe<rg.Curve, ppc.Curve>(cur);
                            curs.Append(revitCur);
                            rg.Plane newPl = Utils.SketchPlaneUtil.GetPlaneForCurve(revitCur);

                            if (plane == null) { plane = newPl; }
                            else if (Math.Abs(plane.Normal.Normalize().DotProduct(newPl.Normal.Normalize()) - 1) > tolerance)
                            {
                                //the two planes are not aligned so throw exception
                                throw new InvalidOperationException("Cannot create a Revit Extrusion because the profile " +
                                    "curves are not in the same plane");
                            }
                        });
                        profile.Append(curs);
                        rg.XYZ dir = ptConv.FromPipe<rg.XYZ, ppg.Vec>(pe.Direction);
                        if(Math.Abs(plane.Normal.Normalize().DotProduct(dir.Normalize()) - 1) > tolerance)
                        {
                            throw new NotImplementedException("Extrusions with direction not perpendicular to curve" +
                                "cannot be imported into revit, try converting it to a mesh before sending through the pipe");
                        }
                        return PipeForRevit.ActiveDocument.FamilyCreate.NewExtrusion(false, profile,
                            rg.SketchPlane.Create(PipeForRevit.ActiveDocument, plane), pe.Height);
                    }
                ));
        }
    }

    public class GeometryConverter: PipeConverter<rg.GeometryObject, IPipeMemberType>
    {
        public CurveConverter CurveConverter { get; private set; }
        public MeshConverter MeshConverter{ get; private set; }
        //public SurfaceConverter SurfaceConverter { get; private set; }

        public GeometryConverter(PointConverter ptConv, PipeConverter<rg.Plane, ppg.Plane> planeConv)
        {
            //converting various types of curves
            CurveConverter = new CurveConverter(ptConv, planeConv);
            AddConverter(CurveConverter);
            MeshConverter = new MeshConverter(ptConv);
            AddConverter(MeshConverter);
            //SurfaceConverter = new SurfaceConverter(ptConv, CurveConverter, MeshConverter);
            //AddConverter(SurfaceConverter);
            /*
             * Commenting out the Revit Polyline conversion because revit does not treat polylines as geometry
             * that means it has to be added as a set of lines, and that makes change tracking and updating 
             * gemetry difficult so I am not supporting polylines until I can think of a better strategy.
             */
            //converting polylines: for some reasons polyline class doesn't inherit from curves in revit
            //var polylineConv = AddConverter(new PipeConverter<rg.PolyLine, ppc.Polyline>(
            //    (rpl) =>
            //    {
            //        List<rg.XYZ> pts = rpl.GetCoordinates().ToList();
            //        return new ppc.Polyline(pts.Select((pt) => ptConv.ToPipe<rg.XYZ, ppg.Vec>(pt)).ToList());
            //    },
            //    (ppl) =>
            //    {
            //        return rg.PolyLine.Create(ppl.Points.Select((pt) => ptConv.FromPipe<rg.XYZ, ppg.Vec>(pt)).ToList());
            //    }
            //));

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

    public class MeshConverter: PipeConverter<rg.Mesh, ppg.Mesh>
    {
        public MeshConverter(PointConverter ptConv):
            base(
                    (rm) => {
                        ppg.Mesh pm = new ppg.Mesh();
                        pm.Vertices.AddRange(rm.Vertices.Select(pt => ptConv.ToPipe<rg.XYZ, ppg.Vec>(pt)));
                        for(int i = 0; i < rm.NumTriangles; i++)
                        {
                            rg.MeshTriangle tr = rm.get_Triangle(i);
                            rg.XYZ A = tr.get_Vertex(0);
                            rg.XYZ B = tr.get_Vertex(1);
                            rg.XYZ C = tr.get_Vertex(2);
                            pm.Faces.Add(new ulong[] {
                                (ulong)rm.Vertices.IndexOf(A),
                                (ulong)rm.Vertices.IndexOf(B),
                                (ulong)rm.Vertices.IndexOf(C)
                            });
                        }

                        return pm;
                    },
                    (ppm) => {
                        //making sure to triangulate so that revit doesn't fail
                        ppm.Triangulate();

                        rg.TessellatedShapeBuilder builder = new rg.TessellatedShapeBuilder();
                        builder.OpenConnectedFaceSet(true);
                        builder.Target = rg.TessellatedShapeBuilderTarget.Mesh;
                        builder.Fallback = rg.TessellatedShapeBuilderFallback.Salvage;
                        List<rg.XYZ> allVerts = ppm.Vertices.Select((pt) => ptConv.FromPipe<rg.XYZ, ppg.Vec>(pt)).ToList();

                        foreach (var face in ppm.Faces)
                        {
                            List<rg.XYZ> faceVerts = new List<rg.XYZ>();
                            var faceReversed = face.Reverse().ToList();
                            foreach(ulong vertIndex in faceReversed)
                            {
                                faceVerts.Add(allVerts[(int)vertIndex]);
                            }

                            rg.TessellatedFace tface = new rg.TessellatedFace(faceVerts, rg.ElementId.InvalidElementId);
                            //if (!builder.DoesFaceHaveEnoughLoopsAndVertices(tface)) { continue; }
                            builder.AddFace(tface);
                        }
                        builder.CloseConnectedFaceSet();
                        builder.Build();
                        rg.TessellatedShapeBuilderResult result = builder.GetBuildResult();
                        List<rg.GeometryObject> geoms = result.GetGeometricalObjects().ToList();
                        return (rg.Mesh)geoms.FirstOrDefault();
                    }
                ) { }
    }
}
