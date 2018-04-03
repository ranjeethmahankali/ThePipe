using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class NurbsCurve : Curve, IEquatable<NurbsCurve>
    {
        #region-fields
        private List<Vec> _controlPoints;
        private int _degree = 3;
        private List<double> _weights;
        private List<double> _knots;
        private bool _isClosed = false;
        private bool _isPeriodic = false;
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
            set { _knots = NormalizedKnots(value); }
        }
        public bool IsRational
        {
            get { return _weights.All((w) => w == 1); }
        }
        public bool IsClosed
        {
            get { return _isClosed; }
        }
        public bool IsPeriodic { get => _isPeriodic; set => _isPeriodic = value; }
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
            _knots = NormalizedKnots(knots);
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
            return Equals((NurbsCurve)other);
        }

        public bool Equals(NurbsCurve otherCurve)
        {
            if (otherCurve.Degree != _degree) { return false; }
            if (IsPeriodic != otherCurve.IsPeriodic || IsClosed != otherCurve.IsClosed) { return false; }
            if (!Utils.PipeDataUtil.EqualCollections(_controlPoints, otherCurve.ControlPoints) ||
                !Utils.PipeDataUtil.EqualCollections(_weights, otherCurve.Weights)) { return false; }
            return true;
        }

        private static List<double> NormalizedKnots(List<double> knots)
        {
            double min = double.MaxValue, max = double.MinValue;
            foreach(var knot in knots)
            {
                if(knot > max) { max = knot; }
                if(knot < min) { min = knot; }
            }

            if(min == max) { throw new InvalidOperationException("Cannot normalize knots."); }
            return knots.Select((k) => (k - min) / (max - min)).ToList();
        }
        #endregion
    }
}
