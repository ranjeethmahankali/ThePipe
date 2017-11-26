using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhinoPipeConverter;
using Grasshopper.Kernel.Types;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using rh = Rhino.Geometry;

namespace PipeForGrasshopper
{
    public class GHPipeConverter: PipeConverter<IGH_Goo, IPipeMemberType>
    {
        private static Point3dConverter _pt3dConv = new Point3dConverter();
        private static Vector3DConverter _vec3DConv = new Vector3DConverter();
        private static ArcConverter _arcConv = new ArcConverter(_planeConv, _pt3dConv);
        private static PlaneConverter _planeConv = new PlaneConverter(_vec3DConv, _pt3dConv);
        private static LineConverter _lineConv = new LineConverter(_pt3dConv);
        private static CurveConverter _curveConv = new CurveConverter(_pt3dConv, _arcConv, _lineConv);

        private static GHPipeConverter _converter = new GHPipeConverter();
        public GHPipeConverter()
        {
            AddConverter(new PipeConverter<GH_Number, PipeNumber>(
                    (ghNum) => { return new PipeNumber(ghNum.Value); },
                    (pData) => { return new GH_Number(pData.Value); }
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
