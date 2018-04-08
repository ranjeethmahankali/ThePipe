using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry;

namespace PipeDataModel.Utils
{
    public class GeometryUtil
    {
        public static List<double> NormalizedKnots(List<double> knots)
        {
            double min = double.MaxValue, max = double.MinValue;
            for (int i = 0; i < knots.Count; i++)
            {
                //making sure its a non decreasing sequence
                double knot = knots[i];
                if (i > 0 && knots[i - 1] > knot) { knot = knots[i - 1]; }
                //getting the min and max
                if (knot > max) { max = knot; }
                if (knot < min) { min = knot; }
            }

            if (min == max) { throw new InvalidOperationException("Cannot normalize knots."); }
            return knots.Select((k) => (k - min) / (max - min)).ToList();
        }

        public static bool AreCoincident(Vec a, Vec b)
        {
            return Vec.Difference(a, b).Length < 1e-6;
        }

        public static bool ListContainsCoincidentPoint(List<Vec> pts, Vec pt)
        {
            return pts.Any((v) => AreCoincident(v, pt));
        }

        //public List<double> ReconciledKnots(List<double> pipeKnots, List<double> appKnots)
        //{
        //    if(pipeKnots.Count == appKnots.Count) { return pipeKnots; }
        //    if(appKnots.Count < pipeKnots.Count)
        //    {
        //        var normApp = NormalizedKnots(appKnots);
        //        for(int i = 1; i < normApp.Count; i++)
        //        {
                    
        //        }
        //    }
        //    else
        //    {
        //        return NormalizedKnots(appKnots);
        //    }
        //}
    }
}
