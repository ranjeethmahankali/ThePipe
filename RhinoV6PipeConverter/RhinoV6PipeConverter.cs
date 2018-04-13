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


namespace RhinoV6PipeConverter
{
    public class GeometryConverter : PipeConverter<rh.GeometryBase, IPipeMemberType>
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

    public class MeshConverter : PipeConverter<rh.Mesh, pp.Mesh>
    {
        public MeshConverter(Point3fConverter ptConv) :
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

    public class BrepConverter : PipeConverter<rh.Brep, pps.PolySurface>
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

                        return polySurf;
                    },
                    (pb) => {
                        if (pb.Surfaces.Count <= 0) { return null; }
                        rh.Brep brep = null;
                        //trying to create a trimmed brep with built in methods
                        if (Util.TryCreateBrepWithBuiltInMethods(pb, out brep, surfConv, curveConv))
                        {
                            return brep;
                        }

                        //attemping to build it from scratch - 15 attempts
                        int attempts = 0;
                        while(attempts < 15)
                        {
                            brep = new rh.Brep();
                            var ptCloud = new rh.PointCloud(pb.Vertices().Select((p) => ptConv.FromPipe<rh.Point3d, pp.Vec>(p)));
                            var ptList = ptCloud.GetPoints().ToList();
                            ptList.ForEach((p) => brep.Vertices.Add(p, Rhino.RhinoMath.ZeroTolerance));

                            var ppEdges = pb.Edges();
                            foreach (var ppCur in ppEdges)
                            {
                                var rhCurve = curveConv.FromPipe<rh.Curve, ppc.Curve>(ppCur);
                                int curveIndex = brep.Curves3D.Add(rhCurve);
                                int startIndex = ptCloud.ClosestPoint(rhCurve.PointAtStart);
                                int endIndex = ptCloud.ClosestPoint(rhCurve.PointAtEnd);
                                var edge = brep.Edges.Add(startIndex, endIndex, curveIndex, Rhino.RhinoMath.ZeroTolerance);
                            }

                            foreach (var ppSurf in pb.Surfaces)
                            {
                                var rhSurf = surfConv.FromPipe<rh.Surface, pps.Surface>(ppSurf);
                                var surfIndex = brep.AddSurface(rhSurf);
                                var face = brep.Faces.Add(surfIndex);
                                var loop = brep.Loops.Add(rh.BrepLoopType.Outer, face);

                                var surfEdges = ppSurf.Edges();
                                int loopCount = 0;
                                foreach (var ppLoop in surfEdges)
                                {
                                    var rhCurve = curveConv.FromPipe<rh.Curve, ppc.Curve>(ppLoop);
                                    var curve2d = rhSurf.Pullback(rhCurve, Rhino.RhinoMath.ZeroTolerance);
                                    if (curve2d == null) { continue; }
                                    loopCount += 1;

                                    int c2i = brep.Curves2D.Add(curve2d);
                                    int c3i = ppEdges.IndexOf(ppLoop);
                                    if (c3i == -1) { c3i = brep.Curves3D.Add(rhCurve); }

                                    int startIndex = ptCloud.ClosestPoint(rhCurve.PointAtStart);
                                    int endIndex = ptCloud.ClosestPoint(rhCurve.PointAtEnd);
                                    var trim = brep.Trims.Add(brep.Edges[c3i], false, loop, c2i);
                                    trim.IsoStatus = rh.IsoStatus.None;
                                    trim.TrimType = rh.BrepTrimType.Boundary;
                                    trim.SetTolerances(0.0, 0.0);
                                }
                                if (loopCount == 0)
                                {
                                    var curve2d = Util.Get2dEdgeLoop(rhSurf);
                                    int c2i = brep.Curves2D.Add(curve2d);
                                    var curve3d = rhSurf.Pushup(curve2d, Rhino.RhinoMath.ZeroTolerance);
                                    int c3i = brep.Curves3D.Add(curve3d);
                                    int startIndex = ptCloud.ClosestPoint(curve3d.PointAtStart);
                                    int endIndex = ptCloud.ClosestPoint(curve3d.PointAtEnd);
                                    var edge = brep.Edges.Add(startIndex, endIndex, c3i, Rhino.RhinoMath.ZeroTolerance);
                                    var trim = brep.Trims.Add(brep.Edges[c3i], false, loop, c2i);
                                    trim.IsoStatus = rh.IsoStatus.None;
                                    trim.TrimType = rh.BrepTrimType.Boundary;
                                    trim.SetTolerances(0.0, 0.0);
                                }

                                //var uSurfDom = rhSurf.Domain(0);
                                //var vSurfDom = rhSurf.Domain(1);
                                //var ufaceDom = face.Domain(0);
                                //var vfaceDom = face.Domain(1);
                                //var surfNorm = rhSurf.NormalAt(uSurfDom.Mid, vSurfDom.Mid);
                                //var faceNorm = face.NormalAt(ufaceDom.Mid, vfaceDom.Mid);
                                //face.OrientationIsReversed = rh.Vector3d.Multiply(surfNorm, faceNorm) < 0;
                                face.OrientationIsReversed = false;
                            }
                            //updating attempts and breaking if succeeded
                            attempts += 1;
                            if (brep.IsValid) { break; }
                        }

                        string msg;
                        if (!brep.IsValidWithLog(out msg))
                        {
                            System.Diagnostics.Debug.WriteLine(msg);
                            brep.Repair(Rhino.RhinoMath.ZeroTolerance);
                            if (brep.IsValid) { return brep; }

                            //finally attemping to create an untrimmed brep
                            int attempt = 0;
                            while(!brep.IsValid && attempt < 15)
                            {
                                brep = rh.Brep.MergeBreps(pb.Surfaces.Select((s) =>
                                    rh.Brep.CreateFromSurface(surfConv.FromPipe<rh.Surface, pps.Surface>(s))),
                                    Rhino.RhinoMath.ZeroTolerance);
                                attempt += 1;
                            }

                            if (!brep.IsValid)
                            {
                                throw new InvalidOperationException("Failed to create a valid brep from " +
                                    "received data because: \n" + msg);
                            }
                        }
                        return brep;
                    }
                )
        { }
    }
}
