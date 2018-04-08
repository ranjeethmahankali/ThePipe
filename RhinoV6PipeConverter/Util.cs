using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace RhinoV6PipeConverter
{
    public class Util
    {
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
    }
}
