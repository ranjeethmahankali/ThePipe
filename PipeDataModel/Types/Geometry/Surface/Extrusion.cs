using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry;
using PipeDataModel.Types.Geometry.Curve;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public class Extrusion: Surface, IEquatable<Extrusion>
    {
        #region fields
        private Curve.Curve _profile;
        private List<Curve.Curve> _holes = new List<Curve.Curve>();
        private Vec _direction;
        private double _height;
        private bool _cappedAtStart, _cappedAtEnd;
        #endregion

        #region properties
        public Curve.Curve ProfileCurve { get => _profile; set => _profile = value; }
        public Vec Direction { get => _direction; set => _direction = value.Unitized; }
        public double Height { get => _height; set => _height = value; }
        public List<Curve.Curve> Holes { get => _holes; set => _holes = value; }
        public Vec PathVector { get => Vec.Multiply(_direction, _height); }
        public bool CappedAtStart { get => _cappedAtStart; set => _cappedAtStart = value; }
        public bool CappedAtEnd { get => _cappedAtEnd; set => _cappedAtEnd = value; }
        #endregion

        #region constructors
        public Extrusion(Curve.Curve profile, Vec dir, double height)
        {
            _profile = profile;
            Direction = dir;
            _height = height;
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            return Equals((Extrusion)other);
        }
        public bool Equals(Extrusion otherExt)
        {
            return _profile.Equals(otherExt.ProfileCurve) && _direction.Equals(otherExt.Direction) && _height == otherExt.Height;
        }

        public override List<Vec> Vertices()
        {
            var verts = new List<Vec>();
            var profileVerts = _profile.Vertices();
            var topVerts = profileVerts.Select((v) => Vec.Sum(v, Vec.Multiply(Direction, Height))).Reverse();
            verts.AddRange(profileVerts);
            verts.AddRange(topVerts);
            return verts;
        }

        public override List<Curve.Curve> Edges()
        {
            return new List<Curve.Curve>() {
                _profile,
                new Line(_profile.EndPoint, Vec.Sum(_profile.EndPoint, Vec.Multiply(Direction, Height))),
                _profile.Translated(Vec.Multiply(Direction, Height)),
                new Line(_profile.StartPoint, Vec.Sum(_profile.StartPoint, Vec.Multiply(Direction, Height)))
            };
        }
        #endregion
    }
}
