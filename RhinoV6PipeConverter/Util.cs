using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using pps = PipeDataModel.Types.Geometry.Surface;
using ppc = PipeDataModel.Types.Geometry.Curve;
using PipeDataModel.Utils;

namespace RhinoV6PipeConverter
{
    public class Util
    {
        //gets the 2d edge loop for a surface in the parameter space of the surface
        public static Curve Get2dEdgeLoop(Surface surf)
        {
            Interval uDom = surf.Domain(0);
            Interval vDom = surf.Domain(1);

            Point2d A = new Point2d(uDom.T0, vDom.T0);
            Point2d B = new Point2d(uDom.T1, vDom.T0);
            Point2d C = new Point2d(uDom.T1, vDom.T1);
            Point2d D = new Point2d(uDom.T0, vDom.T1);

            List<Curve> curves = new List<Curve>();
            curves.Add(new LineCurve(A, B));
            curves.Add(new LineCurve(B, C));
            curves.Add(new LineCurve(C, D));
            curves.Add(new LineCurve(D, A));

            var loop = new PolyCurve();
            foreach(var curve in curves)
            {
                loop.Append(curve);
            }
            return loop;
        }

        public static bool TryCreateBrepWithBuiltInMethods(pps.PolySurface pb, out Brep brep, SurfaceConverter surfConv, CurveConverter curveConv)
        {
            if (pb.Surfaces.Count <= 0)
            {
                brep = null;
                return true;
            }
            brep = null;
            try
            {
                int attempts = 0;
                while (attempts < 15)
                {
                    if (pb.Surfaces.Count == 1)
                    {
                        var surf = pb.Surfaces.FirstOrDefault();
                        brep = GetTrimmedBrep(surf, surfConv, curveConv);
                    }
                    else
                    {
                        for(int i = 0; i < pb.Surfaces.Count; i++)
                        {
                            pps.Surface s = pb.Surfaces[i];
                            var subrep = GetTrimmedBrep(s, surfConv, curveConv);
                            if (!subrep.IsValid) { subrep.Repair(Rhino.RhinoMath.ZeroTolerance); }
                            if(i == 0) { brep = subrep; }
                            else
                            {
                                //brep = Brep.MergeSurfaces(brep, subrep, Rhino.RhinoMath.ZeroTolerance, 
                                //    Rhino.RhinoMath.ZeroTolerance, Point2d.Unset, Point2d.Unset, 0.0, true) ?? 
                                //    Brep.MergeSurfaces(brep, subrep, Rhino.RhinoMath.ZeroTolerance, Rhino.RhinoMath.ZeroTolerance) ??
                                //    Brep.JoinBreps(new List<Brep>() { brep, subrep }, Rhino.RhinoMath.ZeroTolerance).First();
                                brep = Brep.MergeBreps(new List<Brep>() { brep, subrep }, Rhino.RhinoMath.ZeroTolerance);
                            }
                        }
                    }

                    attempts += 1;
                    //not doing anymore attempts if this time was successful
                    if (!brep.IsValid) { brep.Repair(Rhino.RhinoMath.ZeroTolerance); }
                    if (brep.IsValid) { break; }
                }

                if (!brep.IsValid) { brep.Repair(Rhino.RhinoMath.ZeroTolerance); }
                return brep.IsValid;
            }
            catch(Exception e)
            {
                brep = null;
                return false;
            }
        }

        public static Brep GetEnclosedFacesAsBrep(Brep brep, List<Curve> loops)
        {
            List<Brep> faces = new List<Brep>();
            foreach (var loop in loops)
            {
                AreaMassProperties loopArea = AreaMassProperties.Compute(loop);
                Point3d loopCenter = loopArea.Centroid;
                faces.Add(brep.Faces.OrderBy((f) => {
                    AreaMassProperties faceArea = AreaMassProperties.Compute(f.DuplicateFace(false));
                    return Point3d.Subtract(loopCenter, faceArea.Centroid).Length;
                }).First().DuplicateFace(true));
            }

            if(faces.Count == 0) { return null; }
            else if(faces.Count == 1) { return faces.First(); }
            else
            {
                var result = Brep.MergeBreps(faces, Rhino.RhinoMath.ZeroTolerance) ?? faces.First();
                return result.IsValid ? result : faces.First();
            }
        }

        public static Brep GetTrimmedBrep(pps.Surface surface, SurfaceConverter surfConv, CurveConverter curveConv)
        {
            var rhSurf = surfConv.FromPipe<Surface, pps.Surface>(surface);
            var brep = Brep.CreateFromSurface(rhSurf);
            if (!brep.IsValid) { brep.Repair(Rhino.RhinoMath.ZeroTolerance); }
            if (typeof(pps.NurbsSurface).IsAssignableFrom(surface.GetType())
                    && ((pps.NurbsSurface)surface).TrimCurves.Count > 0)
            {
                List<ppc.Curve> trims = ((pps.NurbsSurface)surface).TrimCurves;
                List<ppc.Curve> loops = brep.Faces.First().Loops.Select((l) =>
                    curveConv.ToPipe<Curve, ppc.Curve>(l.To3dCurve())).ToList();

                if (!PipeDataUtil.EqualIgnoreOrder(loops, trims))
                {
                    var rhTrims = trims.Select((c) => {
                        var cur = curveConv.FromPipe<Curve, ppc.Curve>(c);
                        var cur2d = rhSurf.Pullback(cur, Rhino.RhinoMath.ZeroTolerance);
                        return rhSurf.Pushup(cur2d, Rhino.RhinoMath.ZeroTolerance);
                    }).ToList();
                    var faceToSplit = brep.Faces.First();
                    var brep2 = faceToSplit.Split(rhTrims, Rhino.RhinoMath.ZeroTolerance);
                    if (brep2 != null && !brep2.IsValid) { brep2.Repair(Rhino.RhinoMath.ZeroTolerance); }
                    if (brep2 != null && brep2.IsValid) { brep = GetEnclosedFacesAsBrep(brep2, rhTrims) ?? brep2; }
                }
            }

            return brep;
        }
    }
}
