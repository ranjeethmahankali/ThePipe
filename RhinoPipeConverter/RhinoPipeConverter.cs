using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using PipeDataModel.Utils;
using rh = Rhino.Geometry;
using pp = PipeDataModel.Types.Geometry;
using pps = PipeDataModel.Types.Geometry.Surface;
using ppc = PipeDataModel.Types.Geometry.Curve;


namespace RhinoPipeConverter
{
    public class GeometryConverter:PipeConverter<rh.GeometryBase, IPipeMemberType>
    {
        private static Point3dConverter _pt3dConv = new Point3dConverter();
        private static Point3fConverter _pt3fConv = new Point3fConverter();
        private static Vector3DConverter _vec3DConv = new Vector3DConverter();
        private static PlaneConverter _planeConv = new PlaneConverter(_vec3DConv, _pt3dConv);
        private static ArcConverter _arcConv = new ArcConverter(_planeConv, _pt3dConv);
        private static LineConverter _lineConv = new LineConverter(_pt3dConv);

        public GeometryConverter()
        {
            var ptConv = AddConverter(new PointConverter(_pt3dConv));
            var curveConv = new CurveConverter(_pt3dConv, _arcConv, _lineConv);
            AddConverter(curveConv);
            var meshConv = AddConverter(new MeshConverter(_pt3fConv));

            var surfaceConverter = new SurfaceConverter(curveConv, _vec3DConv, _pt3dConv);
            AddConverter(surfaceConverter);

            var brepConv = new BrepConverter(surfaceConverter, curveConv, _pt3dConv);
            AddConverter(brepConv);
        }
    }

    public class MeshConverter: PipeConverter<rh.Mesh, pp.Mesh>
    {
        public MeshConverter(Point3fConverter ptConv):
            base(
                    (rhm) => {
                        return new pp.Mesh(rhm.Vertices.Select((v) => ptConv.ToPipe<rh.Point3f, pp.Vec>(v)).ToList(),
                            rhm.Faces.Select(
                                (f) => f.IsTriangle ? new ulong[] { (ulong)f.A, (ulong)f.B, (ulong)f.C } : 
                                    new ulong[] { (ulong)f.A, (ulong)f.B, (ulong)f.C, (ulong)f.D }).ToList()
                            );
                    },
                    (ppm) => {
                        var mesh = new rh.Mesh();
                        mesh.Vertices.AddVertices(ppm.Vertices.Select((v) => ptConv.FromPipe<rh.Point3f, pp.Vec>(v)));
                        mesh.Faces.AddFaces(ppm.Faces.Select(
                            (f) => pp.Mesh.FaceIsTriangle(f) ? new rh.MeshFace((int)f[0], (int)f[1], (int)f[2]) : 
                                new rh.MeshFace((int)f[0], (int)f[1], (int)f[2], (int)f[3])
                        ));
                        return mesh;
                    }
                )
        { }
    }

    public class BrepConverter: PipeConverter<rh.Brep, pps.PolySurface>
    {
        public BrepConverter(SurfaceConverter surfConv, CurveConverter curveConv, Point3dConverter ptConv) : 
            base(
                    (rb) => {
                        List<pps.Surface> faces = new List<pps.Surface>();
                        List<List<int>> adjacency = new List<List<int>>();
                        for (int i = 0; i < rb.Faces.Count; i++)
                        {
                            var surf = (pps.NurbsSurface)surfConv.ToPipe<rh.Surface, pps.Surface>(rb.Faces[i].ToNurbsSurface());
                            surf.OuterTrims.Clear();
                            surf.OuterTrims.AddRange(rb.Faces[i].Loops.Where((l) =>
                                l.LoopType == rh.BrepLoopType.Outer).Select((l) =>
                                curveConv.ToPipe<rh.Curve, ppc.Curve>(l.To3dCurve())));
                            surf.InnerTrims.Clear();
                            surf.InnerTrims.AddRange(rb.Faces[i].Loops.Where((l) =>
                                l.LoopType == rh.BrepLoopType.Inner).Select((l) =>
                                curveConv.ToPipe<rh.Curve, ppc.Curve>(l.To3dCurve())));

                            faces.Add(surf);
                            adjacency.Add(rb.Faces[i].AdjacentFaces().ToList());
                        }
                        var polySurf = new pps.PolySurface(faces, adjacency);
                        polySurf.IsSolid = rb.IsSolid;
                        return polySurf;
                    },
                    (pb) => {
                        if(pb.Surfaces.Count <= 0) { return null; }
                        rh.Brep brep = null;
                        int attempts = 0;
                        while(attempts < 15)
                        {
                            if (pb.Surfaces.Count == 1)
                            {
                                var surf = pb.Surfaces.FirstOrDefault();
                                brep = Util.GetTrimmedBrep(surf, surfConv, curveConv);
                            }
                            else
                            {
                                for (int i = 0; i < pb.Surfaces.Count; i++)
                                {
                                    pps.Surface s = pb.Surfaces[i];
                                    var subrep = Util.GetTrimmedBrep(s, surfConv, curveConv);
                                    //if (!subrep.IsValid) { subrep.Repair(Rhino.RhinoMath.ZeroTolerance); }
                                    if (i == 0) { brep = subrep; }
                                    else
                                    {
                                        //brep = Brep.MergeSurfaces(brep, subrep, Rhino.RhinoMath.ZeroTolerance, 
                                        //    Rhino.RhinoMath.ZeroTolerance, Point2d.Unset, Point2d.Unset, 0.0, true) ?? 
                                        //    Brep.MergeSurfaces(brep, subrep, Rhino.RhinoMath.ZeroTolerance, Rhino.RhinoMath.ZeroTolerance) ??
                                        //    Brep.JoinBreps(new List<Brep>() { brep, subrep }, Rhino.RhinoMath.ZeroTolerance).First();
                                        brep = rh.Brep.MergeBreps(new List<rh.Brep>() { brep, subrep }, Rhino.RhinoMath.ZeroTolerance);
                                    }
                                }
                            }

                            attempts += 1;
                            //not doing anymore attempts if this time was successful
                            if (brep.IsValid) { break; }
                        }

                        string msg;
                        if(!brep.IsValidWithLog(out msg))
                        {
                            System.Diagnostics.Debug.WriteLine(msg);
                            attempts = 0;
                            while(!brep.IsValid && attempts < 15)
                            {
                                brep = rh.Brep.MergeBreps(pb.Surfaces.Select((s) =>
                                    rh.Brep.CreateFromSurface(surfConv.FromPipe<rh.Surface, pps.Surface>(s))),
                                    Rhino.RhinoMath.ZeroTolerance);
                                attempts += 1;
                            }
                            
                            if (brep.IsValid) { return brep; }
                            throw new InvalidOperationException("Failed to create a valid brep from " +
                                "received data because: \n" + msg);
                        }
                        return brep;
                    }
                )
        { }
    }
}
