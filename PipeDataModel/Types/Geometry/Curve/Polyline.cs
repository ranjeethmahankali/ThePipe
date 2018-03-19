using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
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

        public override Vec StartPoint
        {
            get { return (_points == null || _points.Count == 0) ? null : _points.First(); }
        }

        public override Vec EndPoint
        {
            get { return (_points == null || _points.Count == 0) ? null : _points.Last(); }
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

        public List<Line> ExplodedLines()
        {
            List<Line> lines = new List<Line>();
            for(int i = 1; i < _points.Count; i++)
            {
                lines.Add(new Line(_points[i - 1], _points[i]));
            }
            return lines;
        }
        #endregion
    }
}
