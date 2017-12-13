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
        private List<double> _weights;
        private List<double> _knots;
        private bool _isClosed = false;
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

        public List<double> Weights
        {
            get { return _weights; }
        }
        public List<double> Knots
        {
            get { return _knots; }
        }
        public bool IsRational
        {
            get { return _weights.All((w) => w == 1); }
        }
        public bool IsClosed
        {
            get { return _isClosed; }
        }
        #endregion

        #region-constructors
        public NurbsCurve(List<Vec> controlPts, int degree)
        {
            _controlPoints = controlPts;
            _degree = degree;
            _weights = new List<double>();
            for(int i = 0; i < _controlPoints.Count; i++)
            {
                _weights.Add(1);
            }
        }
        public NurbsCurve(List<Vec> controlPts, int degree, bool isClosed)
            :this(controlPts, degree)
        {
            _isClosed = isClosed;
        }
        public NurbsCurve(List<Vec> controlPts, int degree, List<double> weights, List<double> knots)
            :this(controlPts, degree)
        {
            _weights = weights;
            _knots = knots;
        }
        public NurbsCurve(List<Vec> controlPts, int degree, List<double> weights, List<double> knots, bool isClosed)
            :this(controlPts, degree, weights, knots)
        {
            _isClosed = isClosed;
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
