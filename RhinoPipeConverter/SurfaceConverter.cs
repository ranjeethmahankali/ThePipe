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
    public class SurfaceConverter : PipeConverter<rh.Surface, pps.Surface>
    {
        public SurfaceConverter(CurveConverter curveConv, Vector3DConverter vecConv, Point3dConverter ptConv)
        {
            //extrusion surfaces
            AddConverter(new PipeConverter<rh.Extrusion, pps.Extrusion>(
                (rhE) => {
                    ppc.Line path = (ppc.Line)curveConv.ConvertToPipe<rh.Curve, ppc.Curve>(rhE.PathLineCurve());
                    
                    pps.Extrusion extr = new pps.Extrusion(curveConv.ConvertToPipe<rh.Curve, ppc.Curve>(rhE.Profile3d(0, 0)),
                        vecConv.ConvertToPipe<rh.Vector3d, pp.Vec>(rhE.PathTangent), path.Length);

                    for (int i = 1; i < rhE.ProfileCount; i++)
                    {
                        extr.Holes.Add(curveConv.ConvertToPipe<rh.Curve, ppc.Curve>(rhE.Profile3d(i, 0)));
                    }
                    return extr;
                },
                (ppE) => {
                    if(1 - ppE.Direction.Dot(new pp.Vec(0, 0, 1)) > 1e-3)
                    {
                        //the extrusion is not vertical
                        throw new InvalidOperationException("Cannot create this extrusion. " +
                            "Try converting it into a polysurface and pushing it again");
                    }
                    var profile = curveConv.FromPipe<rh.Curve, ppc.Curve>(ppE.ProfileCurve);
                    rh.Extrusion extr = rh.Extrusion.Create(profile, ppE.Height, true);
                    ppE.Holes.ForEach((h) => extr.AddInnerProfile(curveConv.FromPipe<rh.Curve, ppc.Curve>(h)));
                    //extr.SetOuterProfile(profile, false);
                    //extr.SetPathAndUp(profile.PointAtStart, profile.PointAtStart + pathVec, pathVec);

                    string msg;
                    if(!extr.IsValidWithLog(out msg))
                    {
                        System.Diagnostics.Debug.WriteLine(msg);
                    }
                    return extr;
                }
            ));

            //NurbsSurfaces
            AddConverter(new PipeConverter<rh.NurbsSurface, pps.NurbsSurface>(
                (rns) =>
                {
                    pps.NurbsSurface nurbs = new pps.NurbsSurface(rns.Points.CountU, rns.Points.CountV, rns.Degree(0), rns.Degree(1));
                    
                    for (int u = 0; u < rns.Points.CountU; u++)
                    {
                        for (int v = 0; v < rns.Points.CountU; v++)
                        {
                            nurbs.SetControlPoint(ptConv.ToPipe<rh.Point3d, pp.Vec>(rns.Points.GetControlPoint(u, v).Location), u, v);
                            nurbs.SetWeight(rns.Points.GetControlPoint(u, v).Weight, u, v);
                        }
                    }

                    nurbs.UKnots = rns.KnotsU.ToList();
                    nurbs.VKnots = rns.KnotsV.ToList();
                    return nurbs;
                },
                (pns) => {
                    var nurbs = rh.NurbsSurface.Create(3, true, pns.UDegree + 1, pns.VDegree + 1, pns.UCount, pns.VCount);

                    for (int u = 0; u < pns.UCount; u++)
                    {
                        for (int v = 0; v < pns.VCount; v++)
                        {
                            var cp = new rh.ControlPoint(ptConv.FromPipe<rh.Point3d, pp.Vec>(pns.GetControlPointAt(u, v)), 
                                pns.GetWeightAt(u,v));
                            nurbs.Points.SetControlPoint(u, v, cp);
                        }
                    }

                    nurbs.KnotsU.CreateUniformKnots(1);
                    nurbs.KnotsV.CreateUniformKnots(1);
                    string msg;
                    if(!nurbs.IsValidWithLog(out msg))
                    {
                        System.Diagnostics.Debug.WriteLine(msg);
                    }
                    return nurbs;
                }
            ));
        }
    }
}
