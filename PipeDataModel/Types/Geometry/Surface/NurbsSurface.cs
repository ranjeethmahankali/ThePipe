using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public class NurbsSurface : Surface, IEquatable<NurbsSurface>
    {
        #region fields
        //the points are stored in a dictionary to make the u,v indexing easy. The key is a hashed integer of u and v
        private Dictionary<int, Vec> _points = new Dictionary<int, Vec>();
        private Dictionary<int, double> _weights = new Dictionary<int, double>();
        private List<double> _uKnots, _vKnots = new List<double>();
        private int _uDegree, _vDegree;
        private int _uCount, _vCount;
        private bool _isClosedInU, _isClosedInV;
        private List<Curve.Curve> _trimCurves = new List<Curve.Curve>();
        #endregion

        #region properties
        public Dictionary<int, Vec> Points { get => _points; set => _points = value; }
        public int UDegree { get => _uDegree; set => _uDegree = value; }
        public int VDegree { get => _vDegree; set => _vDegree = value; }
        public int UCount { get => _uCount; set => _uCount = value; }
        public int VCount { get => _vCount; set => _vCount = value; }
        public List<double> UKnots { get => _uKnots; set => _uKnots = Utils.GeometryUtil.NormalizedKnots(value); }
        public List<double> VKnots { get => _vKnots; set => _vKnots = Utils.GeometryUtil.NormalizedKnots(value); }
        public bool IsClosedInU { get => _isClosedInU; set => _isClosedInU = value; }
        public bool IsClosedInV { get => _isClosedInV; set => _isClosedInV = value; }
        public List<Curve.Curve> TrimCurves { get => _trimCurves; }
        #endregion

        #region constructors
        public NurbsSurface(int uCount, int vCount, int uDeg, int vDeg)
        {
            _uDegree = uDeg;
            _vDegree = vDeg;
            _uCount = uCount;
            _vCount = vCount;
        }
        public NurbsSurface(IEnumerable<Vec> points, int uCount, int vCount, int uDeg, int vDeg):this(uCount, vCount, uDeg, vDeg)
        {
            if(points.Count() != uCount * vCount) { throw new InvalidOperationException("Invalid data for creating a NurbsSurface"); }

            _points.Clear();
            for(int u = 0; u < _uCount; u++)
            {
                for (int v = 0; v < _vCount; v++)
                {
                    int i = u * _vCount + v;
                    _points[HashIndices(u, v)] = _points[i];
                }
            }
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType()) || other == null) { return false; }
            return Equals((NurbsSurface)other);
        }
        public bool Equals(NurbsSurface surf)
        {
            return surf.UDegree == _uDegree && surf.VDegree == _vDegree && surf.UCount == _uCount && surf.VCount == _vCount
                && Utils.PipeDataUtil.EqualCollections(_points, surf.Points)
                && Utils.PipeDataUtil.EqualIgnoreOrder(_trimCurves, surf.TrimCurves);
        }

        public Vec GetControlPointAt(int u, int v)
        {
            Vec pt;
            if(!ValidUVIndices(u,v)) { throw new InvalidOperationException("Invalid u, v indices"); }
            if(_points.TryGetValue(HashIndices(u,v), out pt)) { return pt; }
            return null;
        }


        public void SetControlPoint(Vec pt, int u, int v)
        {
            if (!ValidUVIndices(u, v)) { throw new InvalidOperationException("Invalid u, v indices"); }
            int hash = HashIndices(u, v);
            if (_points.ContainsKey(hash)) { _points[hash] = pt; }
            else { _points.Add(hash, pt); }
        }

        public double GetWeightAt(int u, int v)
        {
            double pt;
            if (!ValidUVIndices(u, v)) { throw new InvalidOperationException("Invalid u, v indices"); }
            if (_weights.TryGetValue(HashIndices(u, v), out pt)) { return pt; }
            return 1;
        }

        public void SetWeight(double weight, int u, int v)
        {
            if (!ValidUVIndices(u, v)) { throw new InvalidOperationException("Invalid u, v indices"); }
            int hash = HashIndices(u, v);
            if (_weights.ContainsKey(hash)) { _weights[hash] = weight; }
            else { _weights.Add(hash, weight); }
        }

        private bool ValidUVIndices(int u, int v)
        {
            return 0 <= u && u < _uCount && 0 <= v && v < _vCount;
        }

        public bool WrapPointsToCloseSurface(int direction)
        {
            if(direction == 0)
            {
                int u = _uCount++;
                for(int v = 0; v < _vCount; v++)
                {
                    Vec pt = GetControlPointAt(0, v);
                    SetControlPoint(pt, u, v);
                }
            }
            else if(direction == 1)
            {
                int v = _vCount++;
                for (int u = 0; u < _uCount; u++)
                {
                    Vec pt = GetControlPointAt(u, 0);
                    SetControlPoint(pt, u, v);
                }
            }
            else { return false; }

            return true;
        }
        private static int HashIndices(int u, int v)
        {
            //seed
            int hash = 19;
            hash = hash * 17 + u;
            hash = hash * 17 + v;
            return hash;
        }
        #endregion
    }
}
