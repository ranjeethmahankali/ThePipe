using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
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
                        Dictionary<int, List<int>> edgeMap = new Dictionary<int, List<int>>();
                        for(int i = 0; i < rb.Faces.Count; i++)
                        {
                            faces.Add(surfConv.ToPipe<rh.Surface, pps.Surface>(rb.Faces[i].ToNurbsSurface()));
                            edgeMap.Add(i, rb.Faces[i].AdjacentEdges().ToList());
                        }

                        List<ppc.Curve> edges = rb.Edges.Select((e) => curveConv.ToPipe<rh.Curve, ppc.Curve>(e.ToNurbsCurve())).ToList();
                        var polySurf = new pps.PolySurface(faces, edges,
                            rb.Vertices.Select((v) => ptConv.ToPipe<rh.Point3d, pp.Vec>(v.Location)).ToList());
                        foreach(int key in edgeMap.Keys)
                        {
                            polySurf.SetEdgeIndices(key, edgeMap[key]);
                        }
                        //incomplete - have to add brep edge classes to the data model
                        return polySurf;
                    },
                    (pb) => {
                        var brep = new rh.Brep();
                        List<rh.Curve> curveList = new List<rh.Curve>();
                        pb.Edges.ForEach((e) => {
                            var curve = curveConv.FromPipe<rh.Curve, ppc.Curve>(e);
                            curveList.Add(curve);
                            var vertex1 = brep.Vertices.Add(ptConv.FromPipe<rh.Point3d, pp.Vec>(e.StartPoint), 
                                Rhino.RhinoMath.ZeroTolerance);
                            var vertex2 = brep.Vertices.Add(ptConv.FromPipe<rh.Point3d, pp.Vec>(e.EndPoint),
                                Rhino.RhinoMath.ZeroTolerance);
                            var edge = brep.Edges.Add(vertex1, vertex2, brep.AddEdgeCurve(curve), Rhino.RhinoMath.ZeroTolerance);
                        });
                        for(int i = 0; i < pb.Surfaces.Count; i++)
                        {
                            var surf = surfConv.FromPipe<rh.Surface, pps.Surface>(pb.Surfaces[i]);
                            var face = brep.Faces.Add(brep.AddSurface(surf));
                            var loop = face.Loops.Add(rh.BrepLoopType.Outer);
                            //var loop1 = face.Loops.Add(rh.BrepLoopType.Outer, face);
                            
                            List<int> edgeIndices = pb.GetEdgeIndices(i);
                            foreach(int ei in edgeIndices)
                            {
                                var trim = loop.Trims.Add(brep.Edges[ei], false, loop, brep.AddTrimCurve(curveList[ei]));
                                string msgTrim;
                                if (!trim.IsValidWithLog(out msgTrim))
                                {
                                    System.Diagnostics.Debug.WriteLine(msgTrim);
                                }
                            }
                            string msgLoop;
                            if (!loop.IsValidWithLog(out msgLoop))
                            {
                                System.Diagnostics.Debug.WriteLine(msgLoop);
                            }
                            string msgFace;
                            if (!face.IsValidWithLog(out msgFace))
                            {
                                System.Diagnostics.Debug.WriteLine(msgFace);
                            }
                        }

                        string msg;
                        if (!brep.IsValidWithLog(out msg))
                        {
                            System.Diagnostics.Debug.WriteLine(msg);
                        }
                        //incomplete - have to add brep edge classes to the data model
                        return brep;
                    }
                )
        { }
    }
}
