using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{   
    [Serializable]
    public class Line: Curve, IEquatable<Line>
    {
        #region-fields
        private Vec _startPt;
        private Vec _endPt;
        #endregion
        #region-properties
        public override Vec StartPoint
        {
            get { return _startPt; }
        }
        public override Vec EndPoint
        {
            get { return _endPt; }
        }
        public double Length
        {
            get { return Vec.Difference(_startPt, _endPt).Length; }
        }
        #endregion
        #region-constructors
        public Line(Vec start, Vec end)
        {
            Vec.EnsureDimensionMatch(start, end);
            _startPt = start;
            _endPt = end;
        }
        #endregion

        #region-methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            return Equals((Line)other);
            
        }
        public bool Equals(Line otherLine)
        {
            return _startPt.Equals(otherLine.StartPoint) && _endPt.Equals(otherLine.EndPoint);
        }
        #endregion
    }
}
