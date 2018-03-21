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
                    rh.Extrusion extr = (rh.Extrusion)rh.Surface.CreateExtrusion(
                        curveConv.FromPipe<rh.Curve, ppc.Curve>(ppE.ProfileCurve),
                        vecConv.FromPipe<rh.Vector3d, pp.Vec>(ppE.PathVector));
                    ppE.Holes.ForEach((h) => { extr.AddInnerProfile(curveConv.FromPipe<rh.Curve, ppc.Curve>(h)); });
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
                    var nurbs = rh.NurbsSurface.Create(3, true, pns.UDegree, pns.VDegree, pns.UCount, pns.VCount);

                    for (int u = 0; u < pns.UCount; u++)
                    {
                        for (int v = 0; v < pns.VCount; v++)
                        {
                            var cp = new rh.ControlPoint(ptConv.FromPipe<rh.Point3d, pp.Vec>(pns.GetControlPointAt(u, v)), 
                                pns.GetWeightAt(u,v));
                            nurbs.Points.SetControlPoint(u, v, cp);
                        }
                    }

                    return nurbs;
                }
            ));
        }
    }
}
