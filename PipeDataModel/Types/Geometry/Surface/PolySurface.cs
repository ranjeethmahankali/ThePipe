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
        private List<Vec> _vertices = new List<Vec>();

        //this map contains the info of which face is surrounded by which edges... in only stores the indices like in a mesh
        private Dictionary<int, List<int>> _surfToEdgeMap = new Dictionary<int, List<int>>();
        #endregion

        #region properties
        public List<Surface> Surfaces { get => _surfaces; }
        public List<Curve.Curve> Edges { get => _edges; }
        public List<Vec> Vertices { get => _vertices; }
        #endregion

        #region constructors
        public PolySurface(List<Surface> surfaces, List<Curve.Curve> edges, List<Vec> vertices)
        {
            _surfaces.Clear();
            _surfaces.AddRange(surfaces);
            _edges.Clear();
            _edges.AddRange(edges);
            _vertices.Clear();
            _vertices.AddRange(vertices);
        }
        #endregion

        #region methods
        public List<int> GetEdgeIndices(int faceIndex)
        {
            List<int> indices;
            if(_surfToEdgeMap.TryGetValue(faceIndex, out indices)) { return indices; }
            return new List<int>();
        }
        public List<Curve.Curve> GetEdges(int faceIndex)
        {
            List<int> indices = GetEdgeIndices(faceIndex);
            return indices.Select((i) => _edges[i]).ToList();
        }
        public void SetEdgeIndices(int faceIndex, List<int> edgeIndices)
        {
            if (_surfToEdgeMap.ContainsKey(faceIndex)) { _surfToEdgeMap[faceIndex] = edgeIndices; }
            else { _surfToEdgeMap.Add(faceIndex, edgeIndices); }
        }
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            PolySurface ops = (PolySurface)other;
            return Utils.PipeDataUtil.EqualCollections(_surfaces, ops.Surfaces);
        }
        #endregion
    }
}
