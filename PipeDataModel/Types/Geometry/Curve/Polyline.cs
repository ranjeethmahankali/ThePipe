using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    public class Polyline : Curve
    {
        #region-fields
        private List<Vec> _points;
        #endregion

        #region-properties
        public List<Vec> Points
        {
            get { return _points; }
        }
        #endregion

        #region-constructors
        public Polyline(List<Vec> pts)
        {
            _points = pts;
        }
        #endregion

        #region-methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            Polyline otherLine = (Polyline)other;
            if(otherLine.Points.Count != _points.Count) { return false; }
            for(int i = 0; i<_points.Count; i++)
            {
                if (!_points[i].Equals(otherLine.Points[i])) { return false; }
            }
            return true;
        }
        #endregion
    }
}
