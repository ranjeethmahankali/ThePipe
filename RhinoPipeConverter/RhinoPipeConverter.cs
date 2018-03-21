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

            var brepConv = new BrepConverter(surfaceConverter, curveConv);
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
        public BrepConverter(SurfaceConverter surfConv, CurveConverter curveConv) : 
            base(
                    (rb) => {
                        var brep = new pps.PolySurface(rb.Surfaces.Select((s) => surfConv.ToPipe<rh.Surface, pps.Surface>(s)).ToList(),
                            rb.Edges.Select((e) => curveConv.ToPipe<rh.Curve, ppc.Curve>(e)).ToList());
                        

                        //incomplete - have to add brep edge classes to the data model
                        return brep;
                    },
                    (pb) => {
                        var brep = new rh.Brep();
                        pb.Surfaces.ForEach((s) => brep.AddSurface(surfConv.FromPipe<rh.Surface, pps.Surface>(s)));
                        pb.Edges.ForEach((s) => brep.AddEdgeCurve(curveConv.FromPipe<rh.Curve, ppc.Curve>(s)));

                        //incomplete - have to add brep edge classes to the data model
                        return brep;
                    }
                )
        { }
    }
}
