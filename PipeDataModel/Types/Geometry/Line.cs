using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry
{   
    [Serializable]
    public class Line: IPipeMemberType
    {
        #region-fields
        private Vec _startPt;
        private Vec _endPt;
        #endregion
        #region-properties
        public Vec StartPoint
        {
            get { return _startPt; }
            set { _startPt = value; }
        }
        public Vec EndPoint
        {
            get { return _endPt; }
            set { _endPt = value; }
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

        public bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            Line otherLine = (Line)other;
            return _startPt.Equals(otherLine.StartPoint) && _endPt.Equals(otherLine.EndPoint);
        }
        #endregion

        #region-methods
        #endregion
    }
}
