using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using pp = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using pps = PipeDataModel.Types.Geometry.Surface;
using dg = Autodesk.DesignScript.Geometry;
using PipeDataModel.Utils;

namespace PipeForDynamo.Converters
{
    internal class SurfaceConverter: PipeConverter<dg.Surface, pps.Surface>
    {
        internal SurfaceConverter(VectorConverter vecConv, PointConverter ptConv, CurveConverter curveConv)
        {
            //NURBS surface
            /*
             * Nurbs Surfaces from the pipe cannot be converted to dynamo nurbs because they could have trims.
             * In Dynamo trimmed surfaces are instances of the Surface class. So it has to be another converter.
             * This converter has to be a one way mapping, hence one of the conversion delegates is null
             */
            var nurbsConv = new PipeConverter<dg.NurbsSurface, pps.NurbsSurface>(
                (dns) => {
                    var nurbs = new pps.NurbsSurface(dns.NumControlPointsU, dns.NumControlPointsV, dns.DegreeU, dns.DegreeV);
                    dg.Point[][] pts = dns.ControlPoints();
                    double[][] weights = dns.Weights();

                    for (int u = 0; u < dns.NumControlPointsU; u++)
                    {
                        for (int v = 0; v < dns.NumControlPointsV; v++)
                        {
                            nurbs.SetControlPoint(ptConv.ToPipe<dg.Point, pp.Vec>(pts[u][v]), u, v);
                            nurbs.SetWeight(weights[u][v], u, v);
                        }
                    }

                    nurbs.UKnots = dns.UKnots().ToList();
                    nurbs.VKnots = dns.VKnots().ToList();

                    return nurbs;
                },
                null
            );
            AddConverter(nurbsConv);

            /*
             * ThePipe Extrusions can be mapped to surfaces in dynamo but all surfaces should not be mapped to extrusions.
             * So this mapping has to be one way, hence the first conversion delegate is null.
             */
            AddConverter(new PipeConverter<dg.Surface, pps.Extrusion>(
                null, //null because of one way mapping
                (pe) => {
                    var extrVec = pp.Vec.Multiply(pe.Direction, pe.Height);
                    var path = curveConv.FromPipe<dg.Curve, ppc.Curve>(new ppc.Line(pe.ProfileCurve.StartPoint,
                        pp.Vec.Sum(pe.ProfileCurve.StartPoint, extrVec)));
                    var profile = curveConv.FromPipe<dg.Curve, ppc.Curve>(pe.ProfileCurve);

                    var extr = dg.Surface.BySweep(profile, path);
                    if (!profile.IsClosed)
                    {
                        var cutPt = profile.PointAtDistance(1e-4);
                        profile = profile.ParameterTrim(0, profile.ParameterAtPoint(cutPt));
                        var profile2 = dg.PolyCurve.ByJoinedCurves(new List<dg.Curve>() { profile }).CloseWithLine();
                        if (!profile2.IsClosed) { return extr; }
                        profile = profile2;
                    }

                    try
                    {
                        var cap1 = dg.Surface.ByPatch(profile);
                        var cap2 = dg.Surface.ByPatch((dg.Curve)profile.Translate(vecConv.FromPipe<dg.Vector, pp.Vec>(extrVec)));
                        if (pe.CappedAtStart) { extr = dg.PolySurface.ByJoinedSurfaces(new List<dg.Surface>() { extr, cap1 }); }
                        if (pe.CappedAtEnd) { extr = dg.PolySurface.ByJoinedSurfaces(new List<dg.Surface>() { extr, cap2 }); }
                    }
                    catch(Exception e)
                    {
                        //do nothing
                    }

                    return extr;
                }
            ));

            /*
             * Surface is not an abstract class in dynamo, so there needs to be concrete conversion logic for them.
             * They are being mapped to Pipe's NurbsSurface class.
             */
            AddConverter(new PipeConverter<dg.Surface, pps.NurbsSurface>(
                (ds) => {
                    var nurbs = nurbsConv.ToPipe<dg.NurbsSurface, pps.NurbsSurface>(ds.ToNurbsSurface());
                    nurbs.TrimCurves.Clear();
                    nurbs.TrimCurves.AddRange(ds.Edges.Select((edge) => curveConv.ToPipe<dg.Curve, ppc.Curve>(edge.CurveGeometry)));
                    return nurbs;
                },
                (pns) => {

                    List<List<dg.Point>> pts = new List<List<dg.Point>>();
                    List<List<double>> weights = new List<List<double>>();
                    for (int u = 0; u < pns.UCount; u++)
                    {
                        List<dg.Point> ptRow = new List<dg.Point>();
                        List<double> wRow = new List<double>();
                        for (int v = 0; v < pns.VCount; v++)
                        {
                            ptRow.Add(ptConv.FromPipe<dg.Point, pp.Vec>(pns.GetControlPointAt(u, v)));
                            wRow.Add(pns.GetWeightAt(u, v));
                        }
                        pts.Add(ptRow);
                        weights.Add(wRow);
                    }

                    dg.Surface nurbs;
                    try
                    {
                        nurbs = dg.NurbsSurface.ByControlPointsWeightsKnots(pts.Select((r) => r.ToArray()).ToArray(),
                            weights.Select((r) => r.ToArray()).ToArray(), pns.UKnots.ToArray(), pns.VKnots.ToArray(), pns.UDegree, pns.VDegree);
                    }
                    catch (Exception e)
                    {
                        nurbs = dg.NurbsSurface.ByControlPoints(pts.Select((r) => r.ToArray()).ToArray(), pns.UDegree, pns.VDegree);
                    }

                    if (pns.TrimCurves.Count > 0)
                    {
                        nurbs = nurbs.TrimWithEdgeLoops(pns.TrimCurves.Select((t) =>
                            (dg.PolyCurve)curveConv.FromPipe<dg.Curve, ppc.Curve>(t.AsPolyCurve())));
                    }
                    return nurbs;
                }
            ));

            //Polysurfaces
            AddConverter(new PipeConverter<dg.PolySurface, pps.PolySurface>(
                (dps) => {
                    return new pps.PolySurface(dps.Faces.Select((f) => {
                        var dgSurf = f.SurfaceGeometry().ToNurbsSurface();
                        var surf = nurbsConv.ToPipe<dg.NurbsSurface, pps.NurbsSurface>(dgSurf);
                        // add edges as trim curves
                        surf.TrimCurves.Clear();
                        surf.TrimCurves.AddRange(f.Edges.Select((edge) => curveConv.ToPipe<dg.Curve, ppc.Curve>(edge.CurveGeometry)));
                        return (pps.Surface)surf;
                    }).ToList());
                },
                (ps) => {
                    return dg.PolySurface.ByJoinedSurfaces(ps.Surfaces.Select((s) => {
                        var surf = FromPipe<dg.Surface, pps.Surface>(s);
                        if (typeof(pps.NurbsSurface).IsAssignableFrom(s.GetType())
                            && ((pps.NurbsSurface)s).TrimCurves.Count > 0)
                        {
                            surf = surf.TrimWithEdgeLoops(((pps.NurbsSurface)s).TrimCurves.Select((c) =>
                                (dg.PolyCurve)curveConv.FromPipe<dg.Curve, ppc.Curve>(c.AsPolyCurve())));
                        }
                        return surf;
                    }));
                }
            ));
        }
    }
}
