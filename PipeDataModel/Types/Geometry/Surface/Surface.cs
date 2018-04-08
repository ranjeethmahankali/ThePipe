using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public abstract class Surface : IPipeMemberType, IEquatable<Surface>
    {
        #region fields
        private Vec _surfaceNormal = null;
        #endregion

        #region properties
        public virtual Vec SurfaceNormal { get => _surfaceNormal; set => _surfaceNormal = value; }
        #endregion

        public abstract bool Equals(IPipeMemberType other);

        public bool Equals(Surface other)
        {
            return Equals((IPipeMemberType)other);
        }

        public abstract List<Vec> Vertices();
        public abstract List<Curve.Curve> Edges();
    }
}
