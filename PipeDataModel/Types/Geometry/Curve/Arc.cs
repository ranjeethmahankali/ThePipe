using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class Arc : Curve
    {
        #region-fields
        private Plane _plane;
        private double _radius;
        private double _startAngle, _endAngle;
        #endregion

        #region-properties
        public Plane Plane
        {
            get { return _plane; }
            set { _plane = value; }
        }
        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }
        public double StartAngle
        {
            get { return _startAngle; }
            set { _startAngle = value; }
        }
        public double EndAngle
        {
            get { return _endAngle; }
            set { _endAngle = value; }
        }
        #endregion

        #region-constructors
        public Arc(Plane pl, double rad, double startAng, double endAng)
        {
            if(!(0 <= startAng && startAng <= 2*Math.PI && 0 <= endAng && endAng <= 2 * Math.PI
                && startAng != endAng && rad > 0))
            {
                throw new ArgumentException("Invalid parameters provided for the Arc constructor");
            }
            _plane = pl;
            _radius = rad;
            _startAngle = startAng;
            _endAngle = endAng;
            EnsureAlignment();
        }
        #endregion

        #region-methods
        private void EnsureAlignment()
        {
            if(StartAngle == 0) { return; }
            Plane = new Plane(Plane.Origin, Plane.X.RotateAbout(Plane.Z, StartAngle), 
                Plane.Y.RotateAbout(Plane.Z, StartAngle));
            EndAngle = EndAngle - StartAngle;
            StartAngle = 0;
        }
        public override bool Equals(IPipeMemberType other)
        {
            if(GetType() != other.GetType()) { return false; }
            Arc otherArc = (Arc)other;
            return _endAngle == otherArc.EndAngle && _startAngle == otherArc.StartAngle
                && _radius == otherArc.Radius && _plane.Equals(otherArc.Plane);
        }
        #endregion
    }
}
