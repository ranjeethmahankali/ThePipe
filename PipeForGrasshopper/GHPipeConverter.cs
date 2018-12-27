using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if GH_V5
using RhinoPipeConverter;
#elif GH_V6
using RhinoV6PipeConverter;
#else
#error Please set the conditional compilation symbols field in the project properties to either GH_V5 or GH_V6
#endif
using Grasshopper.Kernel.Types;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using pps = PipeDataModel.Types.Geometry.Surface;
using rh = Rhino.Geometry;

namespace PipeForGrasshopper
{
    public class GHPipeConverter: PipeConverter<IGH_Goo, IPipeMemberType>
    {
        private static Point3dConverter _pt3dConv = new Point3dConverter();
        private static Vector3DConverter _vec3DConv = new Vector3DConverter();
        private static Point3fConverter _pt3fConv = new Point3fConverter();
        private static PlaneConverter _planeConv = new PlaneConverter(_vec3DConv, _pt3dConv);
        private static ArcConverter _arcConv = new ArcConverter(_planeConv, _pt3dConv);
        private static LineConverter _lineConv = new LineConverter(_pt3dConv);
        private static CurveConverter _curveConv = new CurveConverter(_pt3dConv, _arcConv, _lineConv);
        private static MeshConverter _meshConv = new MeshConverter(_pt3fConv);
        private static SurfaceConverter _surfConv = new SurfaceConverter(_curveConv, _vec3DConv, _pt3dConv);
        private static BrepConverter _brepConv = new BrepConverter(_surfConv, _curveConv, _pt3dConv);

        private static GHPipeConverter _converter = new GHPipeConverter();
        public GHPipeConverter()
        {
            AddConverter(new PipeConverter<GH_Number, PipeNumber>(
                    (ghNum) => { return new PipeNumber(ghNum.Value); },
                    (pData) => { return new GH_Number(pData.Value); }
                ));
            AddConverter(new PipeConverter<GH_Integer, PipeInteger>(
                    (ghInt) => { return new PipeInteger(ghInt.Value); },
                    (pInt) => { return new GH_Integer(pInt.Value); }
                ));
            AddConverter(new PipeConverter<GH_String, PipeString>(
                    (ghStr) => { return new PipeString(ghStr.Value); },
                    (pData) => { return new GH_String(pData.Value); }
                ));
            AddConverter(new PipeConverter<GH_Point, ppg.Vec>(
                    (ghpt) => { return _pt3dConv.ToPipe<rh.Point3d, ppg.Vec>(ghpt.Value); },
                    (ppt) => { return new GH_Point(_pt3dConv.FromPipe<rh.Point3d, ppg.Vec>(ppt)); }
                ));
            AddConverter(new PipeConverter<GH_Curve, ppc.Curve>(
                    (ghc) => { return _curveConv.ToPipe<rh.Curve, ppc.Curve>(ghc.Value); },
                    (ppc) => { return new GH_Curve(_curveConv.FromPipe<rh.Curve, ppc.Curve>(ppc)); }
                ));
            AddConverter(new PipeConverter<GH_Line, ppc.Line>(
                    (ghl) => { return _lineConv.ToPipe<rh.Line, ppc.Line>(ghl.Value); },
                    (ppl) => { return new GH_Line(_lineConv.FromPipe<rh.Line, ppc.Line>(ppl)); }
                ));
            AddConverter(new PipeConverter<GH_Mesh, ppg.Mesh>(
                    (ghm) => { return _meshConv.ToPipe<rh.Mesh, ppg.Mesh>(ghm.Value); },
                    (ppm) => { return new GH_Mesh(_meshConv.FromPipe<rh.Mesh, ppg.Mesh>(ppm)); }
                ));
            AddConverter(new PipeConverter<GH_Surface, pps.Surface>(
                    (ghs) => { return _surfConv.ToPipe<rh.Surface, pps.Surface>(ghs.Value.Surfaces.FirstOrDefault()); },
                    (pps) => { return new GH_Surface(_surfConv.FromPipe<rh.Surface, pps.Surface>(pps)); }
                ));
            AddConverter(new PipeConverter<GH_Brep, pps.PolySurface>(
                    (ghs) => { return _brepConv.ToPipe<rh.Brep, pps.PolySurface>(ghs.Value); },
                    (pps) => { return new GH_Brep(_brepConv.FromPipe<rh.Brep, pps.PolySurface>(pps)); }
                ));
        }

        public static IGH_Goo FromPipe(IPipeMemberType pp)
        {
            return _converter.FromPipe<IGH_Goo, IPipeMemberType>(pp);
        }
        public static IPipeMemberType ToPipe(IGH_Goo obj)
        {
            return _converter.ToPipe<IGH_Goo, IPipeMemberType>(obj);
        }
    }
}
