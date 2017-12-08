using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class PolyCurve : Curve
    {
        #region-fields
        private List<Curve> _segments;
        #endregion
        #region-properties
        public List<Curve> Segments
        {
            get
            {
                if(_segments == null) { _segments = new List<Curve>(); }
                return _segments;
            }
        }

        public override Vec StartPoint
        {
            get { return (_segments == null || _segments.Count == 0) ? null : _segments.First().StartPoint; }
        }

        public override Vec EndPoint
        {
            get { return (_segments == null || _segments.Count == 0) ? null : _segments.Last().EndPoint; }
        }
        #endregion
        #region-constructors
        public PolyCurve(List<Curve> segments)
        {
            foreach(var seg in segments)
            {
                Segments.Add(seg);
            }
        }
        #endregion
        #region-methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            PolyCurve opc = (PolyCurve)other;
            if(opc.Segments.Count != Segments.Count) { return false; }
            for(int i = 0; i < Segments.Count; i++)
            {
                if (!Segments[i].Equals(opc.Segments[i])) { return false; }
            }
            return true;
        }

        public static bool CheckContinuity(List<Curve> segs)
        {
            double tolerance = 1e-7;
            int count = segs.Count;
            if(count < 2) { return true; }
            for(int i = 0; i < count-1; i++)
            {
                double dist = Vec.Difference(segs[i].EndPoint, segs[i + 1].StartPoint).Length;
                if(dist > tolerance)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
