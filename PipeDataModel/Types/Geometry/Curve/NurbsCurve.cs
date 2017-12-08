using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class NurbsCurve : Curve
    {
        #region-fields
        private List<Vec> _controlPoints;
        private int _degree = 3;
        #endregion

        #region-properties
        public List<Vec> ControlPoints
        {
            get { return _controlPoints; }
        }
        public int Degree
        {
            get { return _degree; }
        }

        public override Vec StartPoint
        {
            get { return (_controlPoints == null || _controlPoints.Count == 0) ? null : _controlPoints.First(); }
        }

        public override Vec EndPoint
        {
            get { return (_controlPoints == null || _controlPoints.Count == 0) ? null : _controlPoints.Last(); }
        }
        #endregion

        #region-constructors
        public NurbsCurve(List<Vec> controlPts, int degree)
        {
            _controlPoints = controlPts;
            _degree = degree;
        }
        #endregion

        #region-methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            NurbsCurve otherCurve = (NurbsCurve)other;
            if(otherCurve.Degree != _degree) { return false; }
            if (otherCurve.ControlPoints.Count != _controlPoints.Count) { return false; }
            for (int i = 0; i < _controlPoints.Count; i++)
            {
                if (!_controlPoints[i].Equals(otherCurve.ControlPoints[i])) { return false; }
            }
            return true;
        }
        #endregion
    }
}
