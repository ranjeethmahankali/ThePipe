using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry.Curve;
using PipeDataModel.Utils;

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
        //normal does not make sense for a polysurface
        public override Vec SurfaceNormal { get => null; set { } }
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

        public override List<Vec> Vertices()
        {
            List<Vec> verts = new List<Vec>();
            Action<Vec> uniqueAdd = (v) => {
                if (!verts.Any((v1) => Vec.Difference(v, v1).Length < 1e-6)) { verts.Add(v); }
            };
            foreach(var surf in _surfaces)
            {
                var surfVerts = surf.Vertices();
                foreach(var svert in surfVerts)
                {
                    if(!GeometryUtil.ListContainsCoincidentPoint(verts, svert)){ verts.Add(svert); }
                }
            }
            return verts;
        }

        public override List<Curve.Curve> Edges()
        {
            List<Curve.Curve> edges = new List<Curve.Curve>();
            foreach(var surf in _surfaces)
            {
                var surfEdges = surf.Edges();
                foreach(var edge in surfEdges)
                {
                    if (!edges.Contains(edge)) { edges.Add(edge); }
                }
            }

            return edges;
        }
        #endregion
    }
}
