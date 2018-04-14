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

        public List<Surface> FlattenedSurfaceList()
        {
            List<Surface> surfaces = new List<Surface>();
            if (typeof(PolySurface).IsAssignableFrom(GetType()))
            {
                foreach(var surf in ((PolySurface)this).Surfaces) { surfaces.AddRange(surf.FlattenedSurfaceList()); }
            }
            else
            {
                surfaces.Add(this);
            }
            return surfaces;
        }

        public PolySurface AsPolySurface()
        {
            if (typeof(PolySurface).IsAssignableFrom(GetType()))
            {
                return (PolySurface)this;
            }
            else
            {
                return new PolySurface(new List<Surface>() { this }, new List<List<int>>() { new List<int>() });
            }
        }
    }
}
