using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry.Curve;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public class PolySurface: Surface
    {
        #region fields
        private List<Surface> _surfaces = new List<Surface>();
        private List<Curve.Curve> _edges = new List<Curve.Curve>(); 
        #endregion

        #region properties
        public List<Surface> Surfaces { get => _surfaces; }
        public List<Curve.Curve> Edges { get => _edges; }
        #endregion

        #region constructors
        public PolySurface(List<Surface> surfaces, List<Curve.Curve> edges)
        {
            _surfaces.Clear();
            _surfaces.AddRange(surfaces);
            _edges.Clear();
            _edges.AddRange(edges);
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            PolySurface ops = (PolySurface)other;
            return Utils.PipeDataUtil.EqualCollections(_surfaces, ops.Surfaces);
        }
        #endregion
    }
}
