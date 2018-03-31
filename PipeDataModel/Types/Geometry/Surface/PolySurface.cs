using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry.Curve;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public class PolySurface: Surface, IEquatable<PolySurface>
    {
        #region fields
        private List<Surface> _surfaces = new List<Surface>();
        #endregion

        #region properties
        public List<Surface> Surfaces { get => _surfaces; }
        #endregion

        #region constructors
        public PolySurface(List<Surface> surfaces)
        {
            _surfaces.Clear();
            _surfaces.AddRange(surfaces);
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            return Equals((PolySurface)other);
        }
        public bool Equals(PolySurface otherPolySurface)
        {
            return Utils.PipeDataUtil.EqualCollections(_surfaces, otherPolySurface.Surfaces);
        }
        #endregion
    }
}
