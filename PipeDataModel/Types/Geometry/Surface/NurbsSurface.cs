using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Surface
{
    public class NurbsSurface : Surface
    {
        #region fields
        private List<Vec> _points;
        private int _uDegree, _vDegree;
        private int _uCount, _vCount;
        #endregion

        #region properties
        public List<Vec> Points { get => _points; set => _points = value; }
        public int UDegree { get => _uDegree; set => _uDegree = value; }
        public int VDegree { get => _vDegree; set => _vDegree = value; }
        public int UCount { get => _uCount; set => _uCount = value; }
        public int VCount { get => _vCount; set => _vCount = value; }
        #endregion

        #region constructors
        public NurbsSurface(IEnumerable<Vec> points, int uCount, int vCount, int uDeg, int vDeg)
        {
            if(points.Count() != uCount * vCount) { throw new InvalidOperationException("Invalid data for creating a NurbsSurface"); }
            _points = points.ToList();
            _uDegree = uDeg;
            _vDegree = vDeg;
            _uCount = uCount;
            _vCount = vCount;
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType()) || other == null) { return false; }
            NurbsSurface surf = (NurbsSurface)other;
            return surf.UDegree == _uDegree && surf.VDegree == _vDegree && surf.UCount == _uCount && surf.VCount == _vCount
                && Utils.PipeDataUtil.EqualCollections(_points, surf.Points);
        }
        #endregion
    }
}
